using NUnit.Framework;
using WeSay.LexicalModel;
using WeSay.LexicalModel.Db4o_Specific;
using WeSay.Project;
using WeSay.UI;
using System.Windows.Forms;

namespace WeSay.LexicalTools.Tests
{
	[TestFixture]
	public class EntryViewControlTests
	{
		LexEntry empty;
		LexEntry apple;
		LexEntry banana;
		LexEntry car;
		LexEntry bike;
		private ViewTemplate _viewTemplate;

		[SetUp]
		public void SetUp()
		{
			Db4oLexModelHelper.InitializeForNonDbTests();
			WeSayWordsProject.InitializeForTests();

			empty = CreateTestEntry("", "", "");
			apple = CreateTestEntry("apple", "red thing", "An apple a day keeps the doctor away.");
			banana = CreateTestEntry("banana", "yellow food", "Monkeys like to eat bananas.");
			car = CreateTestEntry("car", "small motorized vehicle", "Watch out for cars when you cross the street.");
			bike = CreateTestEntry("bike", "vehicle with two wheels", "He rides his bike to school.");

			string[] analysisWritingSystemIds = new string[] { BasilProject.Project.WritingSystems.TestWritingSystemAnalId };
			string[] vernacularWritingSystemIds = new string[] { BasilProject.Project.WritingSystems.TestWritingSystemVernId };
			this._viewTemplate = new ViewTemplate();
			this._viewTemplate.Add(new Field(Field.FieldNames.EntryLexicalForm.ToString(), "LexEntry", vernacularWritingSystemIds));
			this._viewTemplate.Add(new Field(Field.FieldNames.SenseGloss.ToString(), "LexSense", analysisWritingSystemIds));
			this._viewTemplate.Add(new Field(Field.FieldNames.ExampleSentence.ToString(), "LexExampleSentence", vernacularWritingSystemIds));
			this._viewTemplate.Add(new Field(Field.FieldNames.ExampleTranslation.ToString(), "LexExampleSentence", analysisWritingSystemIds));

		}

		[Test]
		public void CreateWithInventory()
		{
			EntryViewControl entryViewControl = new EntryViewControl();
			Assert.IsNotNull(entryViewControl);
		}

		[Test]
		public void NullDataSource_ShowsEmpty()
		{
			EntryViewControl entryViewControl = CreateForm(null);
			Assert.AreEqual(string.Empty, entryViewControl.ControlFormattedView.Text);
		}

		[Test]
		public void FormattedView_ShowsCurrentEntry()
		{
		   TestEntryShows(apple);
		   TestEntryShows(banana);
		}

		private void TestEntryShows(LexEntry entry)
		{
			EntryViewControl entryViewControl = CreateForm(entry);
			Assert.IsTrue(entryViewControl.ControlFormattedView.Text.Contains(GetLexicalForm(entry)));
			Assert.IsTrue(entryViewControl.ControlFormattedView.Text.Contains(GetGloss(entry)));
			Assert.IsTrue(entryViewControl.ControlFormattedView.Text.Contains(GetExampleSentence(entry)));
		}

		[Test, Ignore("For now, we also show the ghost field in this situation.")]
		public void EditField_SingleControl()
		{
			EntryViewControl entryViewControl = CreateFilteredForm(apple, Field.FieldNames.SenseGloss.ToString(), "LexSense", BasilProject.Project.WritingSystems.TestWritingSystemAnalId);
			Assert.AreEqual(1, entryViewControl.ControlEntryDetail.Count);
		}

		[Test]
		public void EditField_SingleControlWithGhost()
		{
			EntryViewControl entryViewControl = CreateFilteredForm(apple, Field.FieldNames.SenseGloss.ToString(), "LexSense", BasilProject.Project.WritingSystems.TestWritingSystemAnalId);
			Assert.AreEqual(2, entryViewControl.ControlEntryDetail.Count);
		}

		[Test]
		public void EditField_MapsToLexicalForm()
		{
			TestEditFieldMapsToLexicalForm(car);
			TestEditFieldMapsToLexicalForm(bike);
		}

		private static void TestEditFieldMapsToLexicalForm(LexEntry entry)
		{
			EntryViewControl entryViewControl = CreateFilteredForm(entry, Field.FieldNames.SenseGloss.ToString(), "LexSense", BasilProject.Project.WritingSystems.TestWritingSystemAnalId);
			DetailList entryDetailControl = entryViewControl.ControlEntryDetail;
			Control referenceControl = entryDetailControl.GetControlOfRow(0);
			Label labelControl = entryDetailControl.GetLabelControlFromReferenceControl(referenceControl);
			Assert.AreEqual("Meaning", labelControl.Text);
			MultiTextControl editControl = (MultiTextControl)entryDetailControl.GetEditControlFromReferenceControl(referenceControl);
			editControl.TextBoxes[0].Text = "test";
			Assert.IsTrue(editControl.TextBoxes[0].Text.Contains(GetGloss(entry)));
		}

		[Test]
		public void EditField_Change_DisplayedInFormattedView()
		{
			EntryViewControl entryViewControl = CreateFilteredForm(apple, Field.FieldNames.EntryLexicalForm .ToString(), "LexEntry", BasilProject.Project.WritingSystems.TestWritingSystemVernId);
			DetailList entryDetailControl = entryViewControl.ControlEntryDetail;
			Control referenceControl = entryDetailControl.GetControlOfRow(0);
			MultiTextControl editControl = (MultiTextControl)entryDetailControl.GetEditControlFromReferenceControl(referenceControl);
			editControl.TextBoxes[0].Text = "test";
			Assert.IsTrue(entryViewControl.ControlFormattedView.Text.Contains("test"));
	   }

		[Test]
		public void EditField_RemoveContents_RemovesSense()
		{
			LexEntry meaningOnly = CreateTestEntry("word", "meaning", "");
			EntryViewControl entryViewControl = CreateForm(meaningOnly);
			DetailList detailList = entryViewControl.ControlEntryDetail;
			MultiTextControl editControl = GetEditControl(detailList, "Meaning");
			editControl.TextBoxes[0].Text = "";

			Assert.IsTrue(GetEditControl(detailList, "Meaning").Name.Contains("ghost"), "Only ghost should remain");
		}

		private static MultiTextControl GetEditControl(DetailList detailList, string labelText) {
			MultiTextControl editControl = null;
			for (int i = 0; i < detailList.Count; i++)
			{
				Control referenceControl = detailList.GetControlOfRow(i);
				Label label = detailList.GetLabelControlFromReferenceControl(referenceControl);
				if(label.Text == labelText)
				{
					editControl = (MultiTextControl)detailList.GetEditControlFromReferenceControl(referenceControl);
					break;
				}
			}
			return editControl;
		}

		[Test]
		public void FormattedView_FocusInControl_Displayed()
		{
			EntryViewControl entryViewControl = CreateFilteredForm(apple, Field.FieldNames.EntryLexicalForm.ToString(), "LexEntry", BasilProject.Project.WritingSystems.TestWritingSystemVernId);
			entryViewControl.ControlFormattedView.Select();
			string rtfOriginal = entryViewControl.ControlFormattedView.Rtf;

			DetailList entryDetailControl = entryViewControl.ControlEntryDetail;
			Control referenceControl = entryDetailControl.GetControlOfRow(0);
			Control editControl = entryDetailControl.GetEditControlFromReferenceControl(referenceControl);

			//JDH added after we added multiple ws's per field. Was: editControl.Select();
			((MultiTextControl)editControl).TextBoxes[0].Select();

			Assert.AreNotEqual(rtfOriginal, entryViewControl.ControlFormattedView.Rtf);
		}

		[Test, Ignore("Not implemented yet.")]
		public void DoSomethingSensibleWhenWSInFieldWasntListedInProjectCollection()
		{
		}

		[Test]
		public void FormattedView_ChangeRecordThenBack_NothingHighlighted()
		{
			EntryViewControl entryViewControl = CreateFilteredForm(apple, Field.FieldNames.EntryLexicalForm.ToString(), "LexEntry", BasilProject.Project.WritingSystems.TestWritingSystemVernId);
			entryViewControl.ControlFormattedView.Select();
			string rtfAppleNothingHighlighted = entryViewControl.ControlFormattedView.Rtf;

			DetailList entryDetailControl = entryViewControl.ControlEntryDetail;
			Control referenceControl = entryDetailControl.GetControlOfRow(0);
			Control editControl = entryDetailControl.GetEditControlFromReferenceControl(referenceControl);

			//JDH added after we added multiple ws's per field. Was: editControl.Select();
			((MultiTextControl)editControl).TextBoxes[0].Select();

			Assert.AreNotEqual(rtfAppleNothingHighlighted, entryViewControl.ControlFormattedView.Rtf);

			entryViewControl.DataSource = banana;
			entryViewControl.DataSource = apple;
//            Debug.WriteLine("Expected: "+rtfAppleNothingHighlighted);
//            Debug.WriteLine("Actual:" + lexFieldControl.ControlFormattedView.Rtf);
			Assert.AreEqual(rtfAppleNothingHighlighted, entryViewControl.ControlFormattedView.Rtf);
		}

		[Test]
		public void FormattedView_EmptyField_StillHighlighted()
		{
			EntryViewControl entryViewControl = CreateFilteredForm(empty, Field.FieldNames.EntryLexicalForm.ToString(), "LexEntry", BasilProject.Project.WritingSystems.TestWritingSystemVernId);
			entryViewControl.ControlFormattedView.Select();
			string rtfEmptyNothingHighlighted = entryViewControl.ControlFormattedView.Rtf;

			DetailList entryDetailControl = entryViewControl.ControlEntryDetail;
			Control referenceControl = entryDetailControl.GetControlOfRow(0);
			Control editControl = entryDetailControl.GetEditControlFromReferenceControl(referenceControl);

			//JDH added after we added multiple ws's per field. Was: editControl.Select();
			((MultiTextControl)editControl).TextBoxes[0].Select();

			Assert.AreNotEqual(rtfEmptyNothingHighlighted, entryViewControl.ControlFormattedView.Rtf);
		}

		private EntryViewControl CreateForm(LexEntry entry)
		{
			EntryViewControl entryViewControl = new EntryViewControl();
			entryViewControl.ViewTemplate = _viewTemplate;
			entryViewControl.DataSource = entry;

			return entryViewControl;
		}


		private static EntryViewControl CreateFilteredForm(LexEntry entry, string field, string className, params string[] writingSystems)
		{
			ViewTemplate viewTemplate = new ViewTemplate();
			viewTemplate.Add(new Field(field, className, writingSystems));
			EntryViewControl entryViewControl = new EntryViewControl();
			entryViewControl.ViewTemplate = viewTemplate;
			entryViewControl.DataSource = entry;
			return entryViewControl;
		}

		private static LexEntry CreateTestEntry(string lexicalForm, string gloss, string exampleSentence)
		{
			LexEntry entry = new LexEntry();
			entry.LexicalForm[GetSomeValidWsIdForField("EntryLexicalForm")] = lexicalForm;
			LexSense sense = (LexSense)entry.Senses.AddNew();
			sense.Gloss[GetSomeValidWsIdForField("SenseGloss")] = gloss;
			LexExampleSentence example = (LexExampleSentence)sense.ExampleSentences.AddNew();
			example.Sentence[GetSomeValidWsIdForField("ExampleSentence")] = exampleSentence;
			return entry;
		}

		private static string GetSomeValidWsIdForField(string fieldName)
		{
			return WeSay.Project.WeSayWordsProject.Project.ViewTemplate.GetField(fieldName).WritingSystemIds[0];
		}

		private static string GetLexicalForm(LexEntry entry)
		{
			return entry.LexicalForm.GetFirstAlternative();
		}

		private static string GetGloss(LexEntry entry)
		{
			return ((LexSense)entry.Senses[0]).Gloss.GetFirstAlternative();
		}

		private static string GetExampleSentence(LexEntry entry)
		{
			return ((LexExampleSentence)((LexSense)entry.Senses[0]).ExampleSentences[0]).Sentence.GetFirstAlternative();
		}
	}
}