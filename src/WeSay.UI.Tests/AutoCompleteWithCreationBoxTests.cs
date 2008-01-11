using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;
using WeSay.Foundation;
using WeSay.Foundation.Options;
using WeSay.Language;
using WeSay.UI;
using WeSay.UI.AutoCompleteTextBox;


namespace WeSay.UI.Tests
{
	[TestFixture]
	public class AutoCompleteWithCreationBoxTests
	{
		private AutoCompleteWithCreationBox<Option, string> _control;
		private Form _window;
		private OptionsList _sourceChoices;
		private List<string> _choiceKeys;
		private Control _somethingElseToFocusOn;
//        private bool _createNewClickedFired;
//        private bool _valueChangedFired;

		/// <summary>
		/// Key concept: this is the data (as would be in the database) that we are editing
		/// </summary>
		private OptionRef _dataBeingEditted;
		private WritingSystem _ws;
		private OptionDisplayAdaptor _displayAdaptor;


		[SetUp]
		public void Setup()
		{
			_ws = new WritingSystem("xx", new Font("Arial", (float)55.9));
//            _createNewClickedFired=false;
//            _valueChangedFired = false;
			_sourceChoices = new OptionsList();
			_choiceKeys = new List<string>();
			AddSourceChoice("one", "1","Notice, this is not the number two.");//nb: key 'two' in there
			AddSourceChoice("two", "2", "A description of two which includes the word duo.");
			AddSourceChoice("three", "3", "A description of this which includes the word trio and is not two.");

			_displayAdaptor = new OptionDisplayAdaptor(_sourceChoices,_ws.Id);
			_control = new AutoCompleteWithCreationBox<Option,string>(CommonEnumerations.VisibilitySetting.Visible);
			_control.Name = "autobox";
			_control.Box.Items = _sourceChoices.Options;
			_control.Box.ItemFilterer = _displayAdaptor.GetItemsToOffer;

		   //leave for individual tests _control.CreateNewClicked += new EventHandler<CreateNewArgs>(_control_CreateNewClicked);
			_control.Box.ItemDisplayStringAdaptor = _displayAdaptor;
			_control.Box.WritingSystem = _ws;
			_control.GetKeyValueFromValue = _displayAdaptor.GetOptionFromKey;
			_control.GetValueFromKeyValue = _displayAdaptor.GetKeyFromOption;
			_control.ValueChanged += new EventHandler(_control_ValueChanged);

			_dataBeingEditted = new OptionRef();
		}


		void _control_CreateNewClicked(object sender, CreateNewArgs e)
		{
			//_createNewClickedFired=true;
		}

		void _control_ValueChanged(object sender, EventArgs e)
		{
		   // _valueChangedFired = true;
		}



		[TearDown]
		public void TearDown()
		{
			if (_window != null)
			{
				_window.Close();
				_window.Dispose();
				_window = null;
			}
		}

		[Test]
		public void ShowWithNothingChosenYet()
		{
			_control.Box.SelectedItem = null;
			BindAndShow();
		}



		[Test]
		public void DisplaysCorrectLabel()
		{
			SetKeyAndShow("3");
			Assert.AreEqual("three", _control.Box.Text);
		}

		[Test]
		public void ShowWithChoiceThatIsNotInList()
		{
			SetKeyAndShow("29");
			Assert.AreEqual("29", _control.Box.Text);
			Assert.IsTrue(_control.HasProblems);
		}


		[Test]
		public void DoesNotShowAddNewButtonWithClosedList()
		{
			SetKeyAndShow("29");
			_control.Focus();
			Assert.IsTrue(_control.ContainsFocus);
			Assert.IsFalse(_control.AddNewButton.Visible);
		}

		[Test]
		public void DoesShowAddNewButtonWithOpenList()
		{
			_control.CreateNewClicked += new EventHandler<CreateNewArgs>(_control_CreateNewClicked);
			SetKeyAndShow("29");
			_control.Focus();
			Assert.IsTrue(_control.ContainsFocus);
			Assert.IsTrue(_control.AddNewButton.Visible);
		}

		[Test]
		public void SetPreviouslyEmptyItem()
		{
			SetKeyAndShow(String.Empty);
			Assert.AreEqual("", _control.Box.Text);
			_control.Box.Focus();
			_control.Box.Paste("two");
			_somethingElseToFocusOn.Focus();
			Assert.AreEqual("2", _dataBeingEditted.Key);
		}

		[Test]
		public void DropdownMatchesStartOfName()
		{
			SetKeyAndShow(String.Empty);
			Assert.AreEqual("", _control.Box.Text);
			_control.Box.Focus();
			SetBoxText("thr");
			Assert.AreEqual(1, _control.Box.FilteredChoicesListBox.Items.Count);
			Assert.AreEqual("three", _control.Box.FilteredChoicesListBox.Items[0].ToString());
			SetBoxText("twxxx");
			Assert.AreEqual(0, _control.Box.FilteredChoicesListBox.Items.Count);
		 }

		[Test]
		public void DropDownMatchesDescription()
		{
			SetKeyAndShow(String.Empty);
			Assert.AreEqual(0, _control.Box.FilteredChoicesListBox.Items.Count);
			SetBoxText("duo");
			Assert.AreEqual(1, _control.Box.FilteredChoicesListBox.Items.Count);
			Assert.AreEqual("two", _control.Box.FilteredChoicesListBox.Items[0].ToString());
		   SetBoxText("includes");
			Assert.AreEqual(2, _control.Box.FilteredChoicesListBox.Items.Count);
		}

		[Test]
		public void DropDownOrdersExactPrefixFirst()
		{
			SetKeyAndShow(String.Empty);
			SetBoxText("tw");
			//the item 'one' has the word 'two' in the description
			Assert.AreEqual("two", _control.Box.FilteredChoicesListBox.Items[0].ToString());
		 }

		[Test]
		public void DropDownDoesntRepeatItems()
		{
			SetKeyAndShow(String.Empty);
			SetBoxText("two");//everything has the word 'two' and the item 'two' also has a matching name
			//the item 'one' has the word 'two' in the description
			Assert.AreEqual(_sourceChoices.Options.Count, _control.Box.FilteredChoicesListBox.Items.Count);
		}

		private void SetBoxText(string text)
		{
			//it seems that just setting text doesn't trigger drop down;
			//just pasting only appends
			_control.Box.SelectionStart = 0;
			_control.Box.SelectionLength = 1000;
			_control.Box.Paste(text);
		}

		[Test]
		public void ClearItem()
		{
			SetKeyAndShow("3");
			Assert.AreEqual("3", _dataBeingEditted.Key);
			_control.Box.Text = "";
			_somethingElseToFocusOn.Focus();
			Assert.IsTrue(String.IsNullOrEmpty(_dataBeingEditted.Key));
		}


		[Test]
		public void ChangeItemBetweenRealChoices()
		{
			SetKeyAndShow("3");
			Assert.AreEqual("3", _dataBeingEditted.Key);
			_control.Box.Text = "two";
			Assert.AreEqual("2", _dataBeingEditted.Key);
		}



		[Test]
		public void SensitiveToFontSize()
		{
			SetKeyAndShow("3");
			//the +3 fudge here is because the actual height of the
			//inner text box is something less than the Font's GetHeight
			Assert.Greater(_control.Height +3, _ws.Font.GetHeight());
		}

		//------------------------------------------------------------
		private void AddSourceChoice(string label, string key, string description)
		{
			MultiText name = new MultiText();
			name[_ws.Id] = label;
			Option item = new Option(key, name);
			item.Description.SetAlternative(_ws.Id, description);
			_sourceChoices.Options.Add(item);
			_choiceKeys.Add(key);
		}


		private void SetKeyAndShow(string key)
		{
			_dataBeingEditted.Key = key;
			BindAndShow();
		}

		/// <summary>
		/// Needed for focus-related tests
		/// </summary>
		private void BindAndShow()
		{
			if (_window == null)
			{
				_control.Box.SelectedItem = _displayAdaptor.GetOptionFromKey(_dataBeingEditted.Key);
				if(_control.Box.SelectedItem == null)
				{
				 _control.Box.Text = _dataBeingEditted.Key;//will show with red background
				}
				//just creating it is enough; the events bindings it creates keeps it alive
				SimpleBinding<string> dummy = new SimpleBinding<string>(_dataBeingEditted, _control);
				_window = new Form();
				_somethingElseToFocusOn = new Button();
				_window.Controls.Add(_somethingElseToFocusOn);

			   // _control.Dock = DockStyle.Fill;
				_window.Controls.Add(_control);

				_window.Show();
			}
		}


	}
}