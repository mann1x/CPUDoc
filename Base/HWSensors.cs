using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUDoc
{
    public class HWSensors<T> : IList<T>
    {
        private List<T> _innerList;
        private IEnumerable<T> _lazyLoader;
        private void ensureList()
        {
            if (_innerList == null)
                _innerList = new List<T>(_lazyLoader);
        }
        #region IList<T> Members
        public int IndexOf(T item)
        {
            ensureList();
            return _innerList.IndexOf(item);
        }
        public void Insert(int index, T item)
        {
            ensureList();
            _innerList.Insert(index, item);
        }
        public void RemoveAt(int index)
        {
            ensureList();
            _innerList.RemoveAt(index);
        }
        public T this[int index]
        {
            get
            {
                ensureList();
                return _innerList[index];
            }
            set
            {
                ensureList();
                _innerList[index] = value;
            }
        }
        #endregion
        #region ICollection<T> Members
        public void Add(T item)
        {
            ensureList();
            _innerList.Add(item);
        }
        public void Clear()
        {
            ensureList();
            _innerList.Clear();
        }
        public bool Contains(T item)
        {
            ensureList();
            return _innerList.Contains(item);
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            ensureList();
            _innerList.CopyTo(array, arrayIndex);
        }
        public int Count
        {
            get { ensureList(); return _innerList.Count; }
        }
        public bool IsReadOnly
        {
            get { return false; }
        }
        public bool Remove(T item)
        {
            ensureList();
            return _innerList.Remove(item);
        }
        #endregion
        #region IEnumerable<T> Members
        public IEnumerator<T> GetEnumerator()
        {
            ensureList();
            return _innerList.GetEnumerator();
        }
        #endregion
        #region IEnumerable Members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            ensureList();
            return _innerList.GetEnumerator();
        }
        #endregion
    }
    public static class HWSensors
    {
        public static void InitZen(this List<HWSensorItem> _sensors, HWSensorName _name, int _offset, double _multi = 1, bool _enabled = true)
        {
            try
            {
                foreach (var _sensor in _sensors)
                {
                    if (_sensor.Name == _name)
                    {
                        _sensor.Source = HWSensorSource.Zen;
                        _sensor.Enabled = _enabled;
                        _sensor.ZenMulti = _multi;
                        _sensor.ZenPTOffset = _offset;
                        App.LogDebug($"Zen Added {_name} Offset {_offset}");
                    }
                }

            }
            catch (Exception ex)
            {
                App.LogExError($"HWSensor InitZen Exception: {ex.Message}", ex);
            }
        }
        public static void InitZenMulti(this List<HWSensorItem> _sensors, HWSensorName _name, int _offset, int _index, double _multi = 1, bool _enabled = true)
        {
            try
            {
                foreach (var _sensor in _sensors)
                {
                    if (_sensor.Name == _name)
                    {
                        _sensor.Source = HWSensorSource.Zen;
                        _sensor.Enabled = _enabled;
                        _sensor.ZenMulti = _multi;
                        _sensor.ZenPTOffset = _offset;

                        if (_sensor.SensorValues != HWSensorValues.Single)
                        {
                            _sensor.MultiValues.Add(new HWSensorMultiValues());
                            _sensor.MultiValues[_index - 1].ZenPTOffset = _offset;
                            App.LogDebug($"ZenMulti Added {_name} Index {_index - 1} Offset {_offset}");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                App.LogExError($"HWSensor InitZenMulti Exception: {ex.Message}", ex);
            }
        }

        public static void InitLibre(this List<HWSensorItem> _sensors, HWSensorName _name, string _libreId, string _libreLabel)
        {
            try
            {
                foreach (var _sensor in _sensors.Where(sensorItem => sensorItem.Source == HWSensorSource.Libre))
                {
                    if (_sensor.Name == _name)
                    {
                        _sensor.Enabled = true;
                        _sensor.LibreIdentifier = _libreId;
                        _sensor.LibreLabel = _libreLabel;
                        App.LogDebug($"Libre Added {_name} Label {_libreLabel} Id {_libreId}");
                    }
                }

            }
            catch (Exception ex)
            {
                App.LogExError($"HWSensor InitLibre Exception: {ex.Message}", ex);
            }
        }

        public static void InitLibreMulti(this List<HWSensorItem> _sensors, HWSensorName _name, string _libreId, string _libreLabel, int _index, string _libreMultiId, string _libreMultiLabel)
        {
            try
            {
                foreach (var _sensor in _sensors.Where(sensorItem => sensorItem.Source == HWSensorSource.Libre))
                {
                    if (_sensor.Name == _name)
                    {
                        _sensor.Enabled = true;
                        _sensor.LibreIdentifier = _libreId;
                        _sensor.LibreLabel = _libreLabel;

                        if (_sensor.SensorValues != HWSensorValues.Single)
                        {
                            _sensor.MultiValues.Add(new HWSensorMultiValues());
                            _sensor.MultiValues[_index - 1].LibreIdentifier = _libreMultiId;
                            _sensor.MultiValues[_index - 1].LibreLabel = _libreMultiLabel;
                            App.LogDebug($"Libre Added {_name} Index {_index - 1} Label {_libreMultiLabel} Id {_libreMultiId}");
                        }


                    }
                }

            }
            catch (Exception ex)
            {
                App.LogExError($"HWSensor InitLibreMulti Exception: {ex.Message}", ex);
            }
        }
        public static bool IsEnabled(this List<HWSensorItem> _sensors, HWSensorName _name)
        {
            try
            {
                foreach (var _sensor in _sensors)
                {
                    if (_sensor.Name == _name)
                    {
                        return _sensor.Enabled;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                App.LogExError($"HWSensor IsEnabled Exception: {ex.Message}", ex);
                return false;
            }
        }
        public static bool SetEnabled(this List<HWSensorItem> _sensors, HWSensorName _name, bool _enabled)
        {
            try
            {
                foreach (var _sensor in _sensors)
                {
                    if (_sensor.Name == _name)
                    {
                        _sensor.Enabled = _enabled;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                App.LogExError($"HWSensor SetEnabled Exception: {ex.Message}", ex);
                return false;
            }
        }
        public static bool IsAny(this List<HWSensorItem> _sensors, HWSensorName _name, int _cpu = -1)
        {
            int _count = 0;
            try
            {
                foreach (var _sensor in _sensors)
                {
                    if (_sensor.Name == _name && _sensor.Enabled)
                    {
                        if (_sensor.SensorValues == HWSensorValues.Single)
                        {
                            if (_sensor.Values.Any())
                            {
                                return true;
                            }
                        }
                        else
                        {
                            _count = _sensor.MultiValues.Count;
                            if (_sensor.MultiValues[_cpu].Values.Any())
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                App.LogExError($"HWSensor IsAny Exception {_name} cpu={_cpu}/{_count}: {ex.Message}", ex);
                return false;
            }
        }
        public static float? GetValue(this List<HWSensorItem> _sensors, HWSensorName _name, int _cpu = -1)
        {
            int _count = 0;
            try
            {
                foreach (var _sensor in _sensors)
                {
                    if (_sensor.Name == _name && _sensor.Enabled)
                    {
                        if (_sensor.SensorValues == HWSensorValues.Single)
                        {
                            if (_sensor.Values.Any())
                            {
                                return _sensor.Values.Last();
                            }
                        }
                        else
                        {
                            _count = _sensor.MultiValues.Count;
                            if (_sensor.MultiValues[_cpu].Values.Any())
                            {
                                return _sensor.MultiValues[_cpu].Values.Last();
                            }
                        }
                    }
                }
                return -99999;
            }
            catch (Exception ex)
            {
                App.LogExError($"HWSensor GetValue Exception {_name} cpu={_cpu}/{_count}: {ex.Message}", ex);
                return -99999;
            }
        }
        public static float? GetMax(this List<HWSensorItem> _sensors, HWSensorName _name, int _cpu = -1)
        {
            try
            {
                foreach (var _sensor in _sensors)
                {
                    if (_sensor.Name == _name && _sensor.Enabled)
                    {
                        if (_sensor.SensorValues == HWSensorValues.Single)
                        {
                            if (_sensor.Values.Any())
                            {
                                return _sensor.Values.Max();
                            }
                        }

                        else
                        {
                            if (_sensor.MultiValues[_cpu].Values.Any())
                            {
                                return _sensor.MultiValues[_cpu].Values.Max();
                            }
                        }
                    }
                }
                return -99999;
            }
            catch (Exception ex)
            {
                App.LogExError($"HWSensor GetMax Exception: {ex.Message}", ex);
                return -99999;
            }
        }

        public static float? GetAvg(this List<HWSensorItem> _sensors, HWSensorName _name, int _cpu = -1)
        {
            try
            {
                foreach (var _sensor in _sensors)
                {
                    if (_sensor.Name == _name && _sensor.Enabled)
                    {
                        if (_sensor.SensorValues == HWSensorValues.Single)
                        {
                            if (_sensor.Values.Any())
                            {
                                return _sensor.Values.Average();
                            }
                        }
                        else
                        {
                            if (_sensor.MultiValues[_cpu].Values.Any())
                            {
                                return _sensor.MultiValues[_cpu].Values.Average();
                            }
                        }
                    }
                }
                return -99999;
            }
            catch (Exception ex)
            {
                App.LogExError($"HWSensor GetAvg Exception: {ex.Message}", ex);
                return -99999;
            }
        }
        public static void SetValueOffset(this List<HWSensorItem> _sensors, HWSensorName _name, float? _offset)
        {
            try
            {
                foreach (var _sensor in _sensors)
                {
                    if (_sensor.Name == _name)
                    {
                        _sensor.ValueOffset = _offset;
                        App.LogDebug($"HWSensor SetValueOffset {_sensor.Name} to {_offset}");
                    }
                }
            }
            catch (Exception ex)
            {
                App.LogExError($"HWSensor SetValueOffset Exception: {ex.Message}", ex);
            }
        }

        public static float? GetLastAvgMulti(this List<HWSensorItem> _sensors, HWSensorName _name)
        {
            try
            {
                foreach (var _sensor in _sensors)
                {
                    if (_sensor.Name == _name && _sensor.Enabled && _sensor.SensorValues != HWSensorValues.Single)
                    {
                        List<float?> _avg = new List<float?>();
                        foreach (var _sensorValues in _sensor.MultiValues)
                        {
                            _avg.Add(_sensorValues.Values.Last());
                        }
                        return _avg.Average();
                    }
                }
                return -99999;
            }
            catch (Exception ex)
            {
                App.LogExError($"HWSensor GetLastAvgMulti Exception: {ex.Message}", ex);
                return -99999;
            }
        }
        public static float? GetOffset(this List<HWSensorItem> _sensors, HWSensorName _name)
        {
            try
            {
                foreach (var _sensor in _sensors)
                {
                    if (_sensor.Name == _name)
                    {
                        return _sensor.ValueOffset;
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                App.LogExError($"HWSensor GetOffset Exception: {ex.Message}", ex);
                return 0;
            }
        }
        public static void UpdateSensor(this List<HWSensorItem> _sensors, string _Identifier, float? _value)
        {
            try
            {
                foreach (var _sensor in _sensors.Where(sensorItem => sensorItem.Source == HWSensorSource.Libre))
                {
                    if (_sensor.SensorValues != HWSensorValues.Single)
                    {
                        foreach (var _mvalue in _sensor.MultiValues)
                        {
                            if (_mvalue.LibreIdentifier == _Identifier)
                            {
                                _mvalue.Values.Add(_value);
                            }
                        }
                    }
                    else
                    {
                        if (_sensor.LibreIdentifier == _Identifier)
                        {
                            //if (_sensor.Name == HWSensorName.CPUPower) App.LogDebug($"\t\tPWR {_value}");
                            _sensor.Values.Add(_value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.LogExError($"HWSensor UpdateSensor Exception: {ex.Message}", ex);
            }
        }

        public static void UpdateSensorLogicalsScore(this List<HWSensorItem> _sensors, HWSensorName _name, int _cpu, float? _value, string _unit = "")
        {
            try
            {
                foreach (var _sensor in _sensors)
                {
                    if (_sensor.Name == _name && _sensor.Enabled)
                    {
                        _sensor.MultiValues[_cpu].Values.Add(_value);
                        _sensor.MultiValues[_cpu].Unit = _unit;
                        //App.LogDebug($"UpdateSensorLogicalsScore Added {_name} CPU {_cpu} Unit {_unit} Value {_value}");
                    }
                }

            }
            catch (Exception ex)
            {
                App.LogExError($"HWSensor UpdateSensorLogicalsScore Exception: {ex.Message}", ex);
            }
        }
        public static void InitSensorLogicalsScore(this List<HWSensorItem> _sensors, HWSensorName _name, string _unit = "", bool _enabled = true)
        {
            try
            {
                foreach (var _sensor in _sensors)
                {
                    if (_sensor.Name == _name)
                    {
                        _sensor.Unit = _unit;
                        _sensor.Source = HWSensorSource.Bench;
                        _sensor.Enabled = _enabled;
                        _sensor.SensorValues = HWSensorValues.MultiLogical;
                        for (int i = 0; i < App.systemInfo.CPULogicalProcessors; ++i)
                        {
                            _sensor.MultiValues.Add(new HWSensorMultiValues());
                            App.LogDebug($"InitSensorLogicalsScore Added {_name} Index {i} Unit {_unit}");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                App.LogExError($"HWSensor InitSensorLogicalsScore Exception: {ex.Message}", ex);
            }
        }
        public static void SetUnit(this List<HWSensorItem> _sensors, HWSensorName _name, string _unit)
        {
            try
            {
                foreach (var _sensor in _sensors)
                {
                    if (_sensor.Name == _name)
                    {
                        _sensor.Unit = _unit;
                    }
                }

            }
            catch (Exception ex)
            {
                App.LogExError($"HWSensor SetUnit Exception: {ex.Message}", ex);
            }
        }
        public static string GetUnit(this List<HWSensorItem> _sensors, HWSensorName _name)
        {
            try
            {
                foreach (var _sensor in _sensors)
                {
                    if (_sensor.Name == _name)
                    {
                        return _sensor.Unit;
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                App.LogExError($"HWSensor GetUnit Exception: {ex.Message}", ex);
                return "";
            }
        }

        public static void UpdateZenSensor(this List<HWSensorItem> _sensors, HWSensorName _name, float? _value, int _core = -1)
        {
            try
            {
                foreach (var _sensor in _sensors.Where(sensorItem => sensorItem.Source == HWSensorSource.Zen && sensorItem.Name == _name))
                {
                    if (_sensor.SensorValues != HWSensorValues.Single)
                    {
                        _sensor.MultiValues[_core].Values.Add(_value);
                    }
                    else
                    {
                        _sensor.Values.Add(_value);
                    }
                }
            }
            catch (Exception ex)
            {
                App.LogExError($"HWSensor UpdateZenSensor Exception: {ex.Message}", ex);
            }
        }

        public static void Reset(this List<HWSensorItem> _sensors)
        {
            try
            {
                foreach (var _sensor in _sensors)
                {
                    _sensor.Values.Clear();
                    _sensor.Values.TrimExcess();

                    if (_sensor.SensorValues != HWSensorValues.Single)
                    {
                        foreach (HWSensorMultiValues _mValues in _sensor.MultiValues)
                        {
                            _mValues.Values.Clear();
                            _mValues.Values.TrimExcess();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                App.LogExError($"HWSensor Reset Exception: {ex.Message}", ex);
            }
        }

    }

    public class HWSensorMultiValues
    {
        public List<float?> Values { get; set; }
        public string LibreLabel { get; set; }
        public string LibreIdentifier { get; set; }
        public string Unit { get; set; }
        public int ZenPTOffset { get; set; }
        public void Reset()
        {
            Values.Clear();
            Values.TrimExcess();

        }
        public HWSensorMultiValues()
        {
            LibreLabel = "";
            LibreIdentifier = "";
            ZenPTOffset = -1;
            Values = new();
        }
    }

    public class HWSensorItem
    {
        public HWSensorSource Source { get; set; }
        public List<float?> Values { get; set; }
        public List<HWSensorMultiValues> MultiValues { get; set; }
        public bool Enabled { get; set; }
        public string LibreLabel { get; set; }
        public string LibreIdentifier { get; set; }
        public string ManualLibreLabel { get; set; }
        public string ManialLibreIdentifier { get; set; }
        public string Unit { get; set; }

        public float? ValueOffset { get; set; }
        public int ZenPTOffset { get; set; }
        public double ZenMulti { get; set; }

        public HWSensorConfig ConfigType { get; set; }
        public HWSensorValues SensorValues { get; set; }
        public HWSensorType SensorType { get; set; }
        public HWSensorName Name { get; set; }
        public HWSensorDevice Device { get; set; }

        public HWSensorItem(HWSensorName _name, HWSensorValues _values, HWSensorConfig _config, HWSensorDevice _device, HWSensorType _type, HWSensorSource _source = HWSensorSource.Libre)
        {
            Name = _name;
            SensorValues = _values;
            ConfigType = _config;
            Device = _device;
            Source = _source;
            SensorType = _type;
            Enabled = false;
            LibreLabel = "";
            LibreIdentifier = "";
            Unit = "";
            ZenPTOffset = 0;
            ZenMulti = 1;
            ValueOffset = 0;
            Values = new();

            if (_values != HWSensorValues.Single)
            {
                MultiValues = new();
            }

            App.LogDebug($"Added {_name}");
        }
    }


    public enum HWSensorSource
    {
        Bench,
        Libre,
        Zen

    }
    public enum HWSensorConfig
    {
        Auto,
        Manual

    }

    public enum HWSensorValues
    {
        Single,
        MultiCore,
        MultiLogical

    }
    public enum HWSensorType
    {
        Clock,
        Voltage,
        Power,
        Amperage,
        Load,
        Temperature,
        Percentage,
        Score
    }
    public enum HWSensorDevice
    {
        Bench,
        CPU,
        MainBoard,
        GPU
    }
    public enum HWSensorName
    {
        CPUClock,
        CPUEffClock,
        CPULoad,
        CPUPower,
        CPUVoltage,
        CoresVoltage,
        CoresPower,
        CPUTemp,
        CPUMBTemp,
        CoresTemp,
        CPUFSB,
        SOCVoltage,
        CCD1Temp,
        CCD2Temp,
        CCDSTemp,
        CPUCoresVoltages,
        CPUCoresClocks,
        CPUCoresEffClocks,
        CPUCoresStretch,
        CPUCoresPower,
        CPUCoresTemps,
        CPUCoresC0,
        CPULogicalsLoad,
        CPULogicalsScores,
        CPUPPT,
        CPUTDC,
        CPUEDC,
        CPUPPTLimit,
        CPUTDCLimit,
        CPUEDCLimit,
        CCD1L3Temp,
        CCD2L3Temp,
        CPUSAVoltage,
        CPUIOVoltage,
    }
    public class DetailsGrid
    {
        public string Label { get; set; }
        public float? Val1 { get; set; }
        public float? Val2 { get; set; }
        public string Val1Scale { get; set; }
        public string Val2Scale { get; set; }
        public bool Bold { get; set; }
        public string Format { get; set; }
        public string Unit { get; set; }
        public DetailsGrid(string _label, float? _val1, float? _val2, bool _bold, string _format, string _unit = "", string _val1scale = "", string _val2scale = "")
        {
            Label = _label;
            Val1 = _val1;
            Val2 = _val2;
            Val1Scale = _val1scale;
            Val2Scale = _val2scale;
            Bold = _bold;
            Format = _format;
            Unit = _unit;
        }
    }

    public class MovingAverage
    {
        private readonly int _length;
        private int _circIndex = -1;
        private bool _filled;
        private readonly double _oneOverLength;
        private readonly double[] _circularBuffer;
        private readonly int[] _maxPos;
        private readonly int[] _minPos;
        private double _total;

        public MovingAverage(int length)
        {
            _length = length;
            _oneOverLength = 1.0 / length;
            _circularBuffer = new double[length];
            _maxPos = new int[length];
            _minPos = new int[length];
        }

        public MovingAverage Update(double value)
        {
            double lostValue = _circularBuffer[_circIndex];
            _circularBuffer[_circIndex] = value;

            if (value > GetAbsMax || GetAbsMax == double.NaN) GetAbsMax = value;
            if (value < GetAbsMin || GetAbsMin == double.NaN) GetAbsMin = value;

            if (_circIndex == 0 && !_filled)
            {
                _minPos[_circIndex] = _circIndex;
                _maxPos[_circIndex] = _circIndex;
                GetMax = value;
                GetMin = value;
            }
            else
            {
                int _prevIndex = _circIndex == 0 ? _length - 1 : _circIndex - 1;

                if (value >= GetMax)
                {
                    GetMax = value;
                    _maxPos[_circIndex] = _circIndex;
                }
                else if (_filled)
                {
                    while (true)
                    {
                        if (_circularBuffer[_maxPos[_prevIndex]] > value || _maxPos[_prevIndex] == _circIndex)
                        {
                            _maxPos[_circIndex] = _maxPos[_prevIndex];
                            break;
                        }
                        else
                        {
                            _prevIndex = _maxPos[_prevIndex];

                        }
                    }
                    GetMax = _circularBuffer[_maxPos[_circIndex]];
                }
                else
                {
                    _maxPos[_circIndex] = _maxPos[_circIndex - 1];
                    GetMax = _circularBuffer[_maxPos[_circIndex]];
                }

                _prevIndex = _circIndex == 0 ? _length - 1 : _circIndex - 1;

                if (value <= GetMin)
                {
                    GetMin = value;
                    _minPos[_circIndex] = _circIndex;
                }
                else if (_filled)
                {
                    int i = 1;
                    while (true)
                    {
                        if (i == _length)
                        {
                            _minPos[_circIndex] = _circIndex;
                            break;
                        }
                        if (_circularBuffer[_minPos[_prevIndex]] <= value || _minPos[_prevIndex] == _circIndex)
                        {
                            _minPos[_circIndex] = _minPos[_prevIndex];
                            break;
                        }
                        else
                        {
                            _prevIndex = _minPos[_prevIndex];

                        }
                        i++;
                    }
                    GetMin = _circularBuffer[_minPos[_circIndex]];
                }
                else
                {
                    _minPos[_circIndex] = _minPos[_circIndex - 1];
                    GetMin = _circularBuffer[_minPos[_circIndex - 1]];
                }


            }

            // Maintain totals for Push function
            _total += value;
            _total -= lostValue;

            // If not yet filled, current value is average for the current elements in array
            if (!_filled)
            {
                Current = _total * (1.0 / (_circIndex + 1));
                return this;
            }

            // Compute the average and maybe max
            double average = 0.0;

            for (int i = 0; i < _circularBuffer.Length; i++)
            {
                average += _circularBuffer[i];
            }

            Current = average * _oneOverLength;
            return this;
        }

        public MovingAverage Push(double value)
        {
            // Apply the circular buffer
            if (++_circIndex == _length)
            {
                _circIndex = 0;
            }

            if (value > GetAbsMax || GetAbsMax == double.NaN) GetAbsMax = value;
            if (value < GetAbsMin || GetAbsMin == double.NaN) GetAbsMin = value;

            if (_circIndex == 0 && !_filled)
            {
                _minPos[_circIndex] = _circIndex;
                _maxPos[_circIndex] = _circIndex;
                GetMax = value;
                GetMin = value;
            }
            else
            {
                int _prevIndex = _circIndex == 0 ? _length - 1 : _circIndex - 1;

                if (value >= GetMax)
                {
                    GetMax = value;
                    _maxPos[_circIndex] = _circIndex;
                }
                else if (_filled)
                {
                    int i = 1;
                    while (true)
                    {
                        if (i == _length)
                        {
                            _maxPos[_circIndex] = _circIndex;
                            break;
                        }
                        if (_circularBuffer[_maxPos[_prevIndex]] >= value || _maxPos[_prevIndex] == _circIndex)
                        {
                            _maxPos[_circIndex] = _maxPos[_prevIndex];
                            break;
                        }
                        else
                        {
                            _prevIndex = _maxPos[_prevIndex];

                        }
                        i++;
                    }
                    GetMax = _circularBuffer[_maxPos[_circIndex]];
                }
                else
                {
                    _maxPos[_circIndex] = _maxPos[_circIndex - 1];
                    GetMax = _circularBuffer[_maxPos[_circIndex - 1]];
                }

                _prevIndex = _circIndex == 0 ? _length - 1 : _circIndex - 1;

                if (value <= GetMin)
                {
                    GetMin = value;
                    _minPos[_circIndex] = _circIndex;
                }
                else if (_filled)
                {
                    int i = 1;
                    while (true)
                    {
                        if (i == _length)
                        {
                            _minPos[_circIndex] = _circIndex;
                            break;
                        }
                        if (_circularBuffer[_minPos[_prevIndex]] <= value || _minPos[_prevIndex] == _circIndex)
                        {
                            _minPos[_circIndex] = _minPos[_prevIndex];
                            break;
                        }
                        else
                        {
                            _prevIndex = _minPos[_prevIndex];

                        }
                        i++;
                    }
                    GetMin = _circularBuffer[_minPos[_circIndex]];
                }
                else
                {
                    _minPos[_circIndex] = _minPos[_circIndex - 1];
                    GetMin = _circularBuffer[_minPos[_circIndex - 1]];
                }

            }

            double lostValue = _circularBuffer[_circIndex];
            _circularBuffer[_circIndex] = value;

            // Compute the average
            _total += value;
            _total -= lostValue;


            // If not yet filled, just return. Current value should be 0.0
            if (!_filled && _circIndex != _length - 1)
            {
                Current = _total * (1.0 / (_circIndex + 1));
                return this;
            }
            else
            {
                // Set a flag to indicate this is the first time the buffer has been filled
                _filled = true;
            }
            Current = _total * _oneOverLength;
            return this;
        }

        public int Length { get { return _length; } }
        public double Current { get; private set; } = 0.0;
        public double GetAbsMax { get; private set; }
        public double GetAbsMin { get; private set; }
        public double GetMax { get; private set; }
        public double GetMin { get; private set; }
        public string PrintValues
        {
            get
            {

                string _return = "";
                for (int i = 0; i < _circularBuffer.Length; i++)
                {
                    _return += " " + i + "_" + _circularBuffer[i];
                }
                _return += " index: " + _circIndex;
                return _return;
            }
        }
        public string PrintMinPos
        {
            get
            {

                string _return = "";
                for (int i = 0; i < _circularBuffer.Length; i++)
                {
                    _return += " " + i + "_" + _minPos[i];
                }
                _return += " index: " + _circIndex;
                return _return;
            }
        }
        public string PrintMaxPos
        {
            get
            {

                string _return = "";
                for (int i = 0; i < _circularBuffer.Length; i++)
                {
                    _return += " " + i + "_" + _maxPos[i];
                }
                _return += " index: " + _circIndex;
                return _return;
            }
        }

    }
    internal static class MovingAverageExtensions
    {
        public static IEnumerable<double> MovingAverage<T>(this IEnumerable<T> inputStream, Func<T, double> selector, int period)
        {
            var ma = new MovingAverage(period);
            foreach (var item in inputStream)
            {
                ma.Push(selector(item));
                yield return ma.Current;
            }
        }

        public static IEnumerable<double> MovingAverage(this IEnumerable<double> inputStream, int period)
        {
            var ma = new MovingAverage(period);
            foreach (var item in inputStream)
            {
                ma.Push(item);
                yield return ma.Current;
            }
        }
    }
}
