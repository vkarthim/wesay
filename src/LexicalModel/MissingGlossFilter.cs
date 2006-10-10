using System;
using System.Collections.Generic;

namespace WeSay.LexicalModel
{
	public class MissingGlossFilter : WeSay.Data.IFilter<LexEntry>
	{
		IList<string> _writingSystemIds;

		public MissingGlossFilter(Field field)
		{
			if(field == null)
			{
				throw new ArgumentNullException();
			}
			if(field.FieldName != "Gloss")
			{
				throw new ArgumentOutOfRangeException("field", "should be Gloss field definition");
			}
			_writingSystemIds = field.WritingSystemIds;
		}

		public MissingGlossFilter(FieldInventory fieldInventory)
		{
			if (fieldInventory == null)
			{
				throw new ArgumentNullException();
			}
			if(!fieldInventory.Contains("Gloss"))
			{
				throw new ArgumentOutOfRangeException("field", "should contain Gloss field definition");
			}
			Field field;
			if (!fieldInventory.TryGetField("Gloss", out field))
			{
				throw new ArgumentOutOfRangeException("field", "should contain Sentence field definition");
			}
			_writingSystemIds = field.WritingSystemIds;
		}

		#region IFilter<LexEntry> Members

		public string Key
		{
			get
			{
				string key = ToString();
				foreach (string writingSystemId in _writingSystemIds)
				{
					key += writingSystemId;
				}
				return key;
			}
		}

		public Predicate<LexEntry> Inquire
		{
			get
			{
				return Filter;
			}
		}

		#endregion
		private bool Filter(LexEntry entry)
		{
			if (entry == null)
			{
				return false;
			}

			bool hasSense = false;
			foreach (LexSense sense in entry.Senses)
			{
				hasSense = true;
				foreach (string writingSystemId in _writingSystemIds)
				{
					if (sense.Gloss[writingSystemId].Length == 0)
					{
						return true;
					}
				}
			}
			return !hasSense;
		}
	}
}
