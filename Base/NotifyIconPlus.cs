using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using User32 = Vanara.PInvoke.User32;
using Shell32 = Vanara.PInvoke.Shell32;



namespace System.Windows.Forms
{
    /// <summary>
    ///  Specifies a component that creates
    ///  an icon in the Windows System Tray. This class cannot be inherited.
    /// </summary>
    [DefaultProperty(nameof(Text))]
    [DefaultEvent(nameof(MouseDoubleClick))]
    public sealed partial class NotifyIconPlus : Component
    {
        internal const int MaxTextSize = 127;
        private static readonly object EVENT_MOUSEDOWN = new object();
        private static readonly object EVENT_MOUSEMOVE = new object();
        private static readonly object EVENT_MOUSEUP = new object();
        private static readonly object EVENT_CLICK = new object();
        private static readonly object EVENT_DOUBLECLICK = new object();
        private static readonly object EVENT_MOUSECLICK = new object();
        private static readonly object EVENT_MOUSEDOUBLECLICK = new object();
        private static readonly object EVENT_BALLOONTIPSHOWN = new object();
        private static readonly object EVENT_BALLOONTIPCLICKED = new object();
        private static readonly object EVENT_BALLOONTIPCLOSED = new object();

        private const int WM_TRAYMOUSEMESSAGE = (int)User32.WindowMessage.WM_USER + 1024;
        private static readonly uint WM_TASKBARCREATED = User32.RegisterWindowMessage("TaskbarCreated");

        private readonly object syncObj = new object();

        private Icon icon;
        private string text = string.Empty;
        private readonly uint id;
        private bool added;
        private NotifyIconPlusNativeWindow window;
        private ContextMenuStrip contextMenuStrip;
        private static uint s_nextId;
        private static Guid itemGuid;
        private object userData;
        private bool doubleClick; // checks if doubleclick is fired

        // Visible defaults to false, but the NotifyIconDesigner makes it seem like the default is
        // true.  We do this because while visible is the more common case, if it was a true default,
        // there would be no way to create a hidden NotifyIcon without being visible for a moment.
        private bool visible;

        /// <summary>
        ///  Initializes a new instance of the <see cref='NotifyIconPlus'/> class.
        /// </summary>
        public NotifyIconPlus(Guid guid)
        {
            id = ++s_nextId;
            itemGuid = guid;
            window = new NotifyIconPlusNativeWindow(this);
            UpdateIcon(visible);
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref='NotifyIconPlus'/> class.
        /// </summary>
        public NotifyIconPlus(Guid guid, IContainer container) : this(guid)
        {
            if (container is null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            container.Add(this);
        }

        [DefaultValue(null)]
        public ContextMenuStrip ContextMenuStrip
        {
            get
            {
                return contextMenuStrip;
            }

            set
            {
                contextMenuStrip = value;
            }
        }

        /// <summary>
        ///  Gets or sets the current
        ///  icon.
        /// </summary>
        [Localizable(true)]
        [DefaultValue(null)]
        public Icon Icon
        {
            get
            {
                return icon;
            }
            set
            {
                if (icon != value)
                {
                    icon = value;
                    UpdateIcon(visible);
                }
            }
        }

        /// <summary>
        ///  Gets or sets the ToolTip text displayed when
        ///  the mouse hovers over a system tray icon.
        /// </summary>
        [Localizable(true)]
        [DefaultValue("")]
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                if (value is null)
                {
                    value = string.Empty;
                }

                if (value is not null && !value.Equals(text))
                {
                    if (value is not null && value.Length > MaxTextSize)
                    {
                        throw new ArgumentOutOfRangeException(nameof(Text), value, "Text too long");
                    }

                    text = value;
                    if (added)
                    {
                        UpdateIcon(true);
                    }
                }
            }
        }

        /// <summary>
        ///  Gets or sets a value indicating whether the icon is visible in the Windows System Tray.
        /// </summary>
        [Localizable(true)]
        [DefaultValue(false)]
        public bool Visible
        {
            get
            {
                return visible;
            }
            set
            {
                if (visible != value)
                {
                    UpdateIcon(value);
                    visible = value;
                }
            }
        }

        [Localizable(false)]
        [Bindable(true)]
        [DefaultValue(null)]
        [TypeConverter(typeof(StringConverter))]
        public object Tag
        {
            get
            {
                return userData;
            }
            set
            {
                userData = value;
            }
        }

        [Localizable(false)]
        [Bindable(true)]
        [DefaultValue(null)]
        [TypeConverter(typeof(Guid))]
        public Guid Guid
        {
            get
            {
                return itemGuid;
            }
            set
            {
                itemGuid = value;
            }
        }

        /// <summary>
        ///  Occurs when the user clicks the icon in the system tray.
        /// </summary>
        public event EventHandler Click
        {
            add => Events.AddHandler(EVENT_CLICK, value);
            remove => Events.RemoveHandler(EVENT_CLICK, value);
        }

        /// <summary>
        ///  Occurs when the user double-clicks the icon in the system tray.
        /// </summary>
        public event EventHandler DoubleClick
        {
            add => Events.AddHandler(EVENT_DOUBLECLICK, value);
            remove => Events.RemoveHandler(EVENT_DOUBLECLICK, value);
        }

        /// <summary>
        ///  Occurs when the user clicks the icon in the system tray.
        /// </summary>
        public event MouseEventHandler MouseClick
        {
            add => Events.AddHandler(EVENT_MOUSECLICK, value);
            remove => Events.RemoveHandler(EVENT_MOUSECLICK, value);
        }

        /// <summary>
        ///  Occurs when the user mouse double clicks the icon in the system tray.
        /// </summary>
        public event MouseEventHandler MouseDoubleClick
        {
            add => Events.AddHandler(EVENT_MOUSEDOUBLECLICK, value);
            remove => Events.RemoveHandler(EVENT_MOUSEDOUBLECLICK, value);
        }

        /// <summary>
        ///  Occurs when the
        ///  user presses a mouse button while the pointer is over the icon in the system tray.
        /// </summary>
        public event MouseEventHandler MouseDown
        {
            add => Events.AddHandler(EVENT_MOUSEDOWN, value);
            remove => Events.RemoveHandler(EVENT_MOUSEDOWN, value);
        }

        /// <summary>
        ///  Occurs
        ///  when the user moves the mouse pointer over the icon in the system tray.
        /// </summary>
        public event MouseEventHandler MouseMove
        {
            add => Events.AddHandler(EVENT_MOUSEMOVE, value);
            remove => Events.RemoveHandler(EVENT_MOUSEMOVE, value);
        }

        /// <summary>
        ///  Occurs when the
        ///  user releases the mouse button while the pointer
        ///  is over the icon in the system tray.
        /// </summary>
        public event MouseEventHandler MouseUp
        {
            add => Events.AddHandler(EVENT_MOUSEUP, value);
            remove => Events.RemoveHandler(EVENT_MOUSEUP, value);
        }

        /// <summary>
        ///  Releases the unmanaged resources used by the <see cref="NotifyIcon" />
        ///  and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        ///  <see langword="true" /> to release both managed and unmanaged resources;
        ///  <see langword="false" /> to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (window is not null)
                {
                    icon = null;
                    Text = string.Empty;
                    UpdateIcon(false);
                    window.DestroyHandle();
                    window = null;
                    contextMenuStrip = null;
                }
            }
            else
            {
                // This same post is done in ControlNativeWindow's finalize method, so if you change
                // it, change it there too.
                //
                if (window is not null && window.Handle != IntPtr.Zero)
                {
                    Vanara.PInvoke.User32.PostMessage(window.Handle, Vanara.PInvoke.User32.WindowMessage.WM_CLOSE);
                    window.ReleaseHandle();
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        ///  This method raised the BalloonTipClicked event.
        /// </summary>
        private void OnBalloonTipClicked()
        {
            ((EventHandler)Events[EVENT_BALLOONTIPCLICKED])?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///  This method raised the BalloonTipClosed event.
        /// </summary>
        private void OnBalloonTipClosed()
        {
            ((EventHandler)Events[EVENT_BALLOONTIPCLOSED])?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///  This method raised the BalloonTipShown event.
        /// </summary>
        private void OnBalloonTipShown()
        {
            ((EventHandler)Events[EVENT_BALLOONTIPSHOWN])?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///  This method actually raises the Click event. Inheriting classes should
        ///  override this if they wish to be notified of a Click event. (This is far
        ///  preferable to actually adding an event handler.) They should not,
        ///  however, forget to call base.onClick(e); before exiting, to ensure that
        ///  other recipients do actually get the event.
        /// </summary>
        private void OnClick(EventArgs e)
        {
            ((EventHandler)Events[EVENT_CLICK])?.Invoke(this, e);
        }

        /// <summary>
        ///  Inheriting classes should override this method to handle this event.
        ///  Call base.onDoubleClick to send this event to any registered event listeners.
        /// </summary>
        private void OnDoubleClick(EventArgs e)
        {
            ((EventHandler)Events[EVENT_DOUBLECLICK])?.Invoke(this, e);
        }

        /// <summary>
        ///  Inheriting classes should override this method to handle this event.
        ///  Call base.OnMouseClick to send this event to any registered event listeners.
        /// </summary>
        private void OnMouseClick(MouseEventArgs mea)
        {
            ((MouseEventHandler)Events[EVENT_MOUSECLICK])?.Invoke(this, mea);
        }

        /// <summary>
        ///  Inheriting classes should override this method to handle this event.
        ///  Call base.OnMouseDoubleClick to send this event to any registered event listeners.
        /// </summary>
        private void OnMouseDoubleClick(MouseEventArgs mea)
        {
            ((MouseEventHandler)Events[EVENT_MOUSEDOUBLECLICK])?.Invoke(this, mea);
        }

        /// <summary>
        ///  Raises the <see cref='MouseDown'/> event.
        ///  Inheriting classes should override this method to handle this event.
        ///  Call base.onMouseDown to send this event to any registered event listeners.
        /// </summary>
        private void OnMouseDown(MouseEventArgs e)
        {
            ((MouseEventHandler)Events[EVENT_MOUSEDOWN])?.Invoke(this, e);
        }

        /// <summary>
        ///  Inheriting classes should override this method to handle this event.
        ///  Call base.onMouseMove to send this event to any registered event listeners.
        /// </summary>
        private void OnMouseMove(MouseEventArgs e)
        {
            ((MouseEventHandler)Events[EVENT_MOUSEMOVE])?.Invoke(this, e);
        }

        /// <summary>
        ///  Inheriting classes should override this method to handle this event.
        ///  Call base.onMouseUp to send this event to any registered event listeners.
        /// </summary>
        private void OnMouseUp(MouseEventArgs e)
        {
            ((MouseEventHandler)Events[EVENT_MOUSEUP])?.Invoke(this, e);
        }

        /// </summary>
        /// <summary>
        ///  Shows the context menu for the tray icon.
        /// </summary>
        private void ShowContextMenu()
        {
            if (contextMenuStrip is not null)
            {
                User32.GetCursorPos(out Vanara.PInvoke.POINT pt);

                // Summary: the current window must be made the foreground window
                // before calling TrackPopupMenuEx, and a task switch must be
                // forced after the call.
                User32.SetForegroundWindow(window.Handle);

                // this will set the context menu strip to be toplevel
                // and will allow us to overlap the system tray
                //contextMenuStrip.ShowInTaskbar(pt.x, pt.y);
            }
        }


        /// <summary>
        ///  Updates the icon in the system tray.
        /// </summary>
        private unsafe void UpdateIcon(bool showIconInTray)
        {
            lock (syncObj)
            {
                // Bail if in design mode...
                //
                if (DesignMode)
                {
                    return;
                }

                window.LockReference(showIconInTray);

                var data = new Shell32.NOTIFYICONDATA()
                {
                    cbSize = (uint)Marshal.SizeOf(new Shell32.NOTIFYICONDATA()),
                    uCallbackMessage = WM_TRAYMOUSEMESSAGE,
                    uFlags = Shell32.NIF.NIF_MESSAGE | Shell32.NIF.NIF_GUID,
                    uID = id,
                    guidItem = itemGuid
                };
                if (showIconInTray)
                {
                    if (window.Handle == IntPtr.Zero)
                    {
                        window.CreateHandle(new CreateParams());
                    }
                }

                data.hwnd = window.Handle;
                if (icon is not null)
                {
                    data.uFlags |= Shell32.NIF.NIF_ICON;
                    data.hIcon = icon.Handle;
                }

                data.uFlags |= Shell32.NIF.NIF_TIP;
                data.szTip = text;

                if (showIconInTray && icon is not null)
                {
                    if (!added)
                    {
                        Shell32.Shell_NotifyIcon(Shell32.NIM.NIM_ADD, data);
                        added = true;
                    }
                    else
                    {
                        Shell32.Shell_NotifyIcon(Shell32.NIM.NIM_MODIFY, data);
                    }
                }
                else if (added)
                {
                    Shell32.Shell_NotifyIcon(Shell32.NIM.NIM_DELETE, data);
                    added = false;
                }
            }
        }

        /// <summary>
        ///  Handles the mouse-down event
        /// </summary>
        private void WmMouseDown(ref Message m, MouseButtons button, int clicks)
        {
            if (clicks == 2)
            {
                OnDoubleClick(new MouseEventArgs(button, 2, 0, 0, 0));
                OnMouseDoubleClick(new MouseEventArgs(button, 2, 0, 0, 0));
                doubleClick = true;
            }

            OnMouseDown(new MouseEventArgs(button, clicks, 0, 0, 0));
        }

        /// <summary>
        ///  Handles the mouse-move event
        /// </summary>
        private void WmMouseMove(ref Message m)
        {
            OnMouseMove(new MouseEventArgs(Control.MouseButtons, 0, 0, 0, 0));
        }

        /// <summary>
        ///  Handles the mouse-up event
        /// </summary>
        private void WmMouseUp(ref Message m, MouseButtons button)
        {
            OnMouseUp(new MouseEventArgs(button, 0, 0, 0, 0));
            //subhag
            if (!doubleClick)
            {
                OnClick(new MouseEventArgs(button, 0, 0, 0, 0));
                OnMouseClick(new MouseEventArgs(button, 0, 0, 0, 0));
            }

            doubleClick = false;
        }

        private void WmTaskbarCreated(ref Message m)
        {
            added = false;
            UpdateIcon(visible);
        }

        public void WndProc(ref Message msg)
        {
            switch ((User32.WindowMessage)msg.Msg)
            {
                case (User32.WindowMessage)WM_TRAYMOUSEMESSAGE:
                    switch ((int)(msg.LParam))
                    {
                        case (int)User32.WindowMessage.WM_LBUTTONDBLCLK:
                            WmMouseDown(ref msg, MouseButtons.Left, 2);
                            break;
                        case (int)User32.WindowMessage.WM_LBUTTONDOWN:
                            WmMouseDown(ref msg, MouseButtons.Left, 1);
                            break;
                        case (int)User32.WindowMessage.WM_LBUTTONUP:
                            WmMouseUp(ref msg, MouseButtons.Left);
                            break;
                        case (int)User32.WindowMessage.WM_MBUTTONDBLCLK:
                            WmMouseDown(ref msg, MouseButtons.Middle, 2);
                            break;
                        case (int)User32.WindowMessage.WM_MBUTTONDOWN:
                            WmMouseDown(ref msg, MouseButtons.Middle, 1);
                            break;
                        case (int)User32.WindowMessage.WM_MBUTTONUP:
                            WmMouseUp(ref msg, MouseButtons.Middle);
                            break;
                        case (int)User32.WindowMessage.WM_MOUSEMOVE:
                            WmMouseMove(ref msg);
                            break;
                        case (int)User32.WindowMessage.WM_RBUTTONDBLCLK:
                            WmMouseDown(ref msg, MouseButtons.Right, 2);
                            break;
                        case (int)User32.WindowMessage.WM_RBUTTONDOWN:
                            WmMouseDown(ref msg, MouseButtons.Right, 1);
                            break;
                        case (int)User32.WindowMessage.WM_RBUTTONUP:
                            if (contextMenuStrip is not null)
                            {
                                ShowContextMenu();
                            }

                            WmMouseUp(ref msg, MouseButtons.Right);
                            break;
                    }

                    break;
                case User32.WindowMessage.WM_COMMAND:
                    if (IntPtr.Zero == msg.LParam)
                    {
                        if (CommandPlus.DispatchID((int)(msg.WParam) & 0xFFFF))
                        {
                            return;
                        }
                    }
                    else
                    {
                        window.DefWndProc(ref msg);
                    }

                    break;

                case User32.WindowMessage.WM_DESTROY:
                    // Remove the icon from the taskbar
                    UpdateIcon(false);
                    break;

                case User32.WindowMessage.WM_INITMENUPOPUP:
                default:
                    if (msg.Msg == (int)WM_TASKBARCREATED)
                    {
                        WmTaskbarCreated(ref msg);
                    }

                    window.DefWndProc(ref msg);
                    break;
            }
        }
    }
    public class NotifyIconPlusNativeWindow : NativeWindow
    {
        internal NotifyIconPlus reference;
        private GCHandle rootRef;

        internal NotifyIconPlusNativeWindow(NotifyIconPlus component)
        {
            this.reference = component;
        }

        ~NotifyIconPlusNativeWindow()
        {
            if (base.Handle != IntPtr.Zero)
            {
                Vanara.PInvoke.User32.PostMessage(base.Handle, 0x10, IntPtr.Zero, IntPtr.Zero);
            }
        }

        public void LockReference(bool locked)
        {
            if (locked)
            {
                if (!this.rootRef.IsAllocated)
                {
                    this.rootRef = GCHandle.Alloc(this.reference, GCHandleType.Normal);
                }
            }
            else if (this.rootRef.IsAllocated)
            {
                this.rootRef.Free();
            }
        }

        protected override void OnThreadException(Exception e)
        {
            Application.OnThreadException(e);
        }

        protected override void WndProc(ref Message m)
        {
            this.reference.WndProc(ref m);
        }
    }

    internal class CommandPlus : WeakReference
    {

        private static CommandPlus[] cmds;
        private static int icmdTry;
        private static object internalSyncObject = new object();
        private const int idMin = 0x00100;
        private const int idLim = 0x10000;

        internal int id;

        public CommandPlus(ICommandExecutor target)
            : base(target, false)
        {
            AssignID(this);
        }

        public virtual int ID
        {
            get
            {
                return id;
            }
        }

        protected static void AssignID(CommandPlus cmd)
        {
            lock (internalSyncObject)
            {
                int icmd;

                if (null == cmds)
                {
                    cmds = new CommandPlus[20];
                    icmd = 0;
                }
                else
                {
                    Debug.Assert(cmds.Length > 0, "why is cmds.Length zero?");
                    Debug.Assert(icmdTry >= 0, "why is icmdTry negative?");

                    int icmdLim = cmds.Length;

                    if (icmdTry >= icmdLim)
                        icmdTry = 0;

                    // First look for an empty slot (starting at icmdTry).
                    for (icmd = icmdTry; icmd < icmdLim; icmd++)
                        if (null == cmds[icmd]) goto FindSlotComplete;
                    for (icmd = 0; icmd < icmdTry; icmd++)
                        if (null == cmds[icmd]) goto FindSlotComplete;

                    // All slots have Command objects in them. Look for a command
                    // with a null referent.
                    for (icmd = 0; icmd < icmdLim; icmd++)
                        if (null == cmds[icmd].Target) goto FindSlotComplete;

                    // Grow the array.
                    icmd = cmds.Length;
                    icmdLim = Math.Min(idLim - idMin, 2 * icmd);

                    if (icmdLim <= icmd)
                    {
                        // Already at maximal size. Do a garbage collect and look again.
                        GC.Collect();
                        for (icmd = 0; icmd < icmdLim; icmd++)
                        {
                            if (null == cmds[icmd] || null == cmds[icmd].Target)
                                goto FindSlotComplete;
                        }
                        throw new ArgumentException("Command not allocated");
                    }
                    else
                    {
                        CommandPlus[] newCmds = new CommandPlus[icmdLim];
                        Array.Copy(cmds, 0, newCmds, 0, icmd);
                        cmds = newCmds;
                    }
                }

                FindSlotComplete:

                cmd.id = icmd + idMin;

                cmds[icmd] = cmd;
                icmdTry = icmd + 1;
            }
        }

        public static bool DispatchID(int id)
        {
            CommandPlus cmd = GetCommandFromID(id);
            if (null == cmd)
                return false;
            return cmd.Invoke();
        }

        protected static void Dispose(CommandPlus cmd)
        {
            lock (internalSyncObject)
            {
                if (cmd.id >= idMin)
                {
                    cmd.Target = null;
                    if (cmds[cmd.id - idMin] == cmd)
                        cmds[cmd.id - idMin] = null;
                    cmd.id = 0;
                }
            }
        }

        public virtual void Dispose()
        {
            if (id >= idMin)
                Dispose(this);
        }

        public static CommandPlus GetCommandFromID(int id)
        {
            lock (internalSyncObject)
            {
                if (null == cmds)
                    return null;
                int i = id - idMin;
                if (i < 0 || i >= cmds.Length)
                    return null;
                return cmds[i];
            }
        }

        public virtual bool Invoke()
        {
            object target = Target;
            if (!(target is ICommandExecutor))
                return false;
            ((ICommandExecutor)target).Execute();
            return true;
        }
    }
}