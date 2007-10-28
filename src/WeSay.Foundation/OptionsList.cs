using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Exortech.NetReflector;

namespace WeSay.Foundation
{
    /// <summary>
    /// Used to refer to this option from a field
    /// </summary>
    public class OptionRefCollection: IParentable,
                                      INotifyPropertyChanged,
                                      ICollection<string>,
                                        IReportEmptiness
    {
        private readonly List<string> _keys;

        /// <summary>
        /// This "backreference" is used to notify the parent of changes. 
        /// IParentable gives access to this during explicit construction.
        /// </summary>
        private WeSayDataObject _parent;

        public OptionRefCollection()
        {
            _keys = new List<string>();
        }

        public bool IsEmpty
        {
            get { return _keys.Count == 0; }
        }

        #region ICollection<string> Members

        void ICollection<string>.Add(string key)
        {
            if (_keys.Contains(key))
            {
                throw new ArgumentOutOfRangeException("key", key,
                        "OptionRefCollection already contains that key");
            }

            Add(key);
        }

        /// <summary>
        /// Removes a key from the OptionRefCollection
        /// </summary>
        /// <param name="key">The OptionRef key to be removed</param>
        /// <returns>true when removed, false when doesn't already exists in collection</returns>
        public bool Remove(string key)
        {
            if (_keys.Remove(key))
            {
                NotifyPropertyChanged();
                return true;
            }
            return false;
        }

        public bool Contains(string key)
        {
            return _keys.Contains(key);
        }

        public int Count
        {
            get { return _keys.Count; }
        }

        public void Clear()
        {
            _keys.Clear();
            NotifyPropertyChanged();
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            _keys.CopyTo(array, arrayIndex);
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _keys.GetEnumerator();
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// For INotifyPropertyChanged
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region IParentable Members

        public WeSayDataObject Parent
        {
            set { _parent = value; }
        }

        #endregion

        protected void NotifyPropertyChanged()
        {
            //tell any data binding
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("option"));
                        //todo
            }

            //tell our parent
            _parent.NotifyPropertyChanged("option");
        }

        /// <summary>
        /// Adds a key to the OptionRefCollection
        /// </summary>
        /// <param name="key">The OptionRef key to be added</param>
        /// <returns>true when added, false when already exists in collection</returns>
        public bool Add(string key)
        {
            if (_keys.Contains(key))
            {
                return false;
            }

            _keys.Add(key);
            NotifyPropertyChanged();
            return true;
        }

        /// <summary>
        /// Adds a set of keys to the OptionRefCollection
        /// </summary>
        /// <param name="keys">A set of keys to be added</param>
        public void AddRange(IEnumerable<string> keys)
        {
            bool changed = false;
            foreach (string key in keys)
            {
                if (_keys.Contains(key))
                {
                    continue;
                }

                _keys.Add(key);
                changed = true;
            }

            if (changed)
            {
                NotifyPropertyChanged();
            }
        }

        #region IReportEmptiness Members

        public bool ShouldHoldUpDeletionOfParentObject
        {
            get { return false; }
        }

        public bool ShouldCountAsFilledForPurposesOfConditionalDisplay
        {
            get { return !(IsEmpty); }
        }

        public bool ShouldBeRemovedFromParentDueToEmptiness
        {
            get
            {
                foreach (string s in _keys)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        return false;   // one non-empty is enough to keep us around
                    }
                }
                return true;
            }
        }

        public void RemoveEmptyStuff()
        {
            List<string> condemened=new List<string>();
            foreach (string s in _keys)
            {
                if(string.IsNullOrEmpty(s))
                {
                    condemened.Add(s);
                }
            }
            foreach (string s in condemened)
            {
                this.Remove(s);
            }
        }

        #endregion
    }

    /// <summary>
    /// Used to refer to this option from a field
    /// </summary>
    public class OptionRef: Annotatable, IParentable, IValueHolder<string>, IReportEmptiness
    {
        private string _humanReadableKey;

        /// <summary>
        /// This "backreference" is used to notify the parent of changes. 
        /// IParentable gives access to this during explicit construction.
        /// </summary>
        private WeSayDataObject _parent;

        public OptionRef() //WeSay.Foundation.WeSayDataObject parent)
        {
            _humanReadableKey = string.Empty;
        }

        public bool IsEmpty
        {
            get { return string.IsNullOrEmpty(Value); }
        }

        #region IParentable Members

        public WeSayDataObject Parent
        {
            set { _parent = value; }
        }

        #endregion

        #region IValueHolder<string> Members

        /// <summary>
        /// For INotifyPropertyChanged
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public string Value
        {
            get { return _humanReadableKey; }
            set
            {
                if(value !=null)
                {
                    _humanReadableKey = value.Trim();
                }
                else
                {
                    _humanReadableKey = null;
                }
                // this.Guid = value.Guid;
                NotifyPropertyChanged();
            }
        }

        #endregion

        private void NotifyPropertyChanged()
        {
            //tell any data binding
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("option"));
                        //todo
            }

            //tell our parent

            if (_parent != null)
            {
                _parent.NotifyPropertyChanged("option");
            }
        }

        #region IReportEmptiness Members

        public bool ShouldHoldUpDeletionOfParentObject
        {
            get { return false; }
        }

        public bool ShouldCountAsFilledForPurposesOfConditionalDisplay
        {
            get { return !IsEmpty; }
        }

        public bool ShouldBeRemovedFromParentDueToEmptiness
        {
            get { return IsEmpty; }
        }

        public void RemoveEmptyStuff()
        {
            if(Value == string.Empty)
            {
                Value = null; // better for matching 'missing' for purposes of missing info task
            }

        }

        #endregion
    }

    public class OptionsListWrapper
    {
        [XmlElement(typeof (Option), ElementName = "option")]
        public List<Option> _options;
    }

    /// <summary>
    /// This is like a PossibilityList in FieldWorks, or RangeSet in Toolbox
    /// </summary>
    [XmlRoot("optionsList")]
    public class OptionsList
    {
        private List<Option> _options;

        public OptionsList()
        {
            _options = new List<Option>();
        }

        /// <summary>
        /// just to get the old xml format (which includes a <options> element around the options) read in
        /// </summary>
        [XmlElement("options")]
        public OptionsListWrapper options
        {
            set { _options = value._options; }
            get
            {
                //                OptionsListWrapper w = new OptionsListWrapper();
                //                w._options = _options;
                //                return w;
                return null;
            }
        }

        [XmlElement(typeof (Option), ElementName = "option")]
        public List<Option> Options
        {
            get { return _options; }
            set { _options = value; }
        }

        public static OptionsList LoadFromFile(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof (OptionsList));
            using (XmlReader reader = XmlReader.Create(path))
            {
                OptionsList list = (OptionsList) serializer.Deserialize(reader);
                reader.Close();

#if DEBUG
                foreach (Option option in list.Options)
                {
                    Debug.Assert(option.Name.Forms != null);
                }
#endif
                return list;
            }
        }

        public void SaveToFile(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof (OptionsList));
            XmlAttributeOverrides overrides = new XmlAttributeOverrides();
            XmlAttributes ignoreAttr = new XmlAttributes();
            ignoreAttr.XmlIgnore = true;
            overrides.Add(typeof (Annotatable), "IsStarred", ignoreAttr);

            using (StreamWriter writer = File.CreateText(path))
            {
                serializer.Serialize(writer, this);
                writer.Close();
            }
        }

        //        static private NetReflectorTypeTable MakeTypeTable()
        //        {
        //            NetReflectorTypeTable t = new NetReflectorTypeTable();
        //            t.Add(typeof(OptionsList));
        //            t.Add(typeof(Option));
        //            t.Add(typeof(MultiText));
        //            t.Add(typeof(LanguageForm));
        //            return t;
        //        }
        public Option GetOptionFromKey(string value)
        {
            foreach (Option option in Options)
            {
                if (option.Key == value)
                    return option;
            }
            return null;
        }
    }

    [XmlRoot("option")]
    public class Option
    {
        private MultiText _abbreviation;
        private MultiText _description;
        private string _humanReadableKey;
        private MultiText _name;
        // private Guid _guid;

        public Option()
                : this(string.Empty, new MultiText()) {}

        public Option(string humanReadableKey, MultiText name) //, Guid guid)
        {
            _humanReadableKey = humanReadableKey;
            _name = name;
        }

        [ReflectorProperty("key", Required = true)]
        [XmlElement("key")]
        public string Key
        {
            get
            {
                if (String.IsNullOrEmpty(_humanReadableKey))
                {
                    return GetDefaultKey(); //don't actually save it yet
                }

                else
                {
                    return _humanReadableKey;
                }
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    _humanReadableKey = GetDefaultKey();
                }
                        //the idea here is, we're delaying setting the key in concrete for as long as possible
                        //this allows the ui to continue to auto-create the key during a ui session.
                else if (value != GetDefaultKey())
                {
                    _humanReadableKey = value;
                }
            }
        }

        [ReflectorProperty("name", typeof (MultiTextSerializorFactory),
                Required = true)]
        [XmlElement("name")]
        public MultiText Name
        {
            get { return _name; }
            set { _name = value; }
        }

        [ReflectorProperty("abbreviation", typeof (MultiTextSerializorFactory),
                Required = false)]
        [XmlElement("abbreviation")]
        public MultiText Abbreviation
        {
            get
            {
                if (_abbreviation == null)
                {
                    return Name;
                }
                return _abbreviation;
            }
            set { _abbreviation = value; }
        }

        [ReflectorProperty("description", typeof (MultiTextSerializorFactory),
                Required = false)]
        [XmlElement("description")]
        public MultiText Description
        {
            get
            {
                if (_description == null)
                {
                    _description = new MultiText();
                }
                return _description;
            }
            set { _description = value; }
        }

        private string GetDefaultKey()
        {
            string name = Name.GetFirstAlternative();
            if (!String.IsNullOrEmpty(name))
            {
                return name;
            }
            return Guid.NewGuid().ToString();
        }

        //        [ReflectorProperty("guid", Required = false)]
        //        public Guid Guid
        //        {
        //            get
        //            {
        //                if (_guid == null || _guid == Guid.Empty)
        //                {
        //                    return Guid.NewGuid();
        //                }
        //                return _guid;
        //            }
        //            set { _guid = value; }
        //        }

        public override string ToString()
        {
            return _name.GetFirstAlternative();
        }

        public object GetDisplayProxy(string writingSystemId)
        {
            return new OptionDisplayProxy(this, writingSystemId);
        }

        #region Nested type: OptionDisplayProxy

        /// <summary>
        /// Gives a monolingual representation of the object for use by a combo-box
        /// </summary>
        public class OptionDisplayProxy
        {
            private readonly string _writingSystemId;
            private Option _option;

            public OptionDisplayProxy(Option option, string writingSystemId)
            {
                _writingSystemId = writingSystemId;
                _option = option;
            }

            public string Key
            {
                get { return _option.Key; }
            }

            public Option UnderlyingOption
            {
                get { return _option; }
                set { _option = value; }
            }

            public override string ToString()
            {
                return _option.Name.GetBestAlternative(_writingSystemId, "*");
            }
        }

        #endregion
    }
}