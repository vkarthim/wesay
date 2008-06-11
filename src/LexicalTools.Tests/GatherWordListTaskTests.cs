using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using Palaso.Reporting;
using WeSay.Data;
using WeSay.Foundation;
using WeSay.Language;
using WeSay.LexicalModel;
using WeSay.LexicalModel.Db4o_Specific;
using WeSay.Project;

namespace WeSay.LexicalTools.Tests
{
	[TestFixture]
	public class GatherWordListTaskTests : TaskBaseTests
	{
		LexEntryRepository _lexEntryRepository;
		private string _wordListFilePath;
		private string _filePath;
		private string[] _words = new string[] {"one","two","three"};
		ViewTemplate _viewTemplate;
		private string _glossingLanguageWSId ;
		private string _vernacularLanguageWSId ;

		[SetUp]
		public void Setup()
		{
			_wordListFilePath = Path.GetTempFileName();
			_filePath = Path.GetTempFileName();
			//Db4oLexModelHelper.InitializeForNonDbTests();
			WeSayWordsProject.InitializeForTests();

			_lexEntryRepository = new LexEntryRepository(_filePath); // InMemoryRecordListManager();
			Db4oLexModelHelper.Initialize(_lexEntryRepository.Db4oDataSource.Data);
			_glossingLanguageWSId = BasilProject.Project.WritingSystems.TestWritingSystemAnalId;
			_vernacularLanguageWSId = BasilProject.Project.WritingSystems.TestWritingSystemVernId;

			File.WriteAllLines(_wordListFilePath, _words);
			_viewTemplate = new ViewTemplate();
			this._viewTemplate.Add(new Field(Field.FieldNames.EntryLexicalForm.ToString(), "LexEntry", new string[] { BasilProject.Project.WritingSystems.TestWritingSystemVernId }));

			this._task = new GatherWordListTask(_lexEntryRepository,
												"label",
												"description",
												_wordListFilePath,
												_glossingLanguageWSId,
												_viewTemplate);
		}

		[TearDown]
		public void TearDown()
		{
			_lexEntryRepository.Dispose();
			File.Delete(_wordListFilePath);
			File.Delete(_filePath);
		}

		[Test]
		public void EmptyTemplate()
		{
			GatherWordListTask g = new GatherWordListTask(_lexEntryRepository,
														  "label",
														  "description",
														  _wordListFilePath,
														  WritingSystem.IdForUnknownAnalysis,
														  new ViewTemplate());

			Assert.IsNotNull(g);
		}

		[Test, ExpectedException(typeof(ErrorReport.NonFatalMessageSentToUserException))]
		public void MissingWordListFileGivesMessage()
		{
			GatherWordListTask g = new GatherWordListTask(_lexEntryRepository,
														  "label",
														  "description",
														  "NotThere.txt",
														  WritingSystem.IdForUnknownAnalysis,
														  new ViewTemplate());
			g.Activate();//should give a box to user, an exception in this text environment
		}

		[Test, ExpectedException(typeof(ErrorReport.NonFatalMessageSentToUserException))]
		public void WritingSystemNotInCurrentListGivesMessage()
		{
			GatherWordListTask g = new GatherWordListTask(_lexEntryRepository,
														  "label",
														  "description",
														  _wordListFilePath,
														  "z7z",
														  new ViewTemplate());
			g.Activate();//should give a box to user, an exception in this text environment
		}

		[Test]
		public void InitiallyWordIsCorrect()
		{
			Assert.AreEqual("one",Task.CurrentWord);
		}

		[Test]
		public void CanNavigateToSecondWord()
		{
			Task.NavigateNext();
			Assert.AreEqual("two", Task.CurrentWord);
		}

		[Test]
		public void CanNavigateBackToFirstWord()
		{
			Task.NavigateNext();
			 Task.NavigatePrevious();
		   Assert.AreEqual("one", Task.CurrentWord);
		}

		[Test]
		public void InitiallyCanNavigateNext()
		{
			Assert.IsTrue(Task.CanNavigateNext);
		}
		[Test]
		public void InitiallyCannotNavigatePrevious()
		{
			Assert.IsFalse(Task.CanNavigatePrevious);
		}
		[Test]
		public void NavigateNextEnabledFalseAtEnd()
		{
			Assert.IsTrue(Task.CanNavigateNext);
			NextToEnd();
			Assert.IsFalse(Task.CanNavigateNext);
	   }

		private void NextToEnd()
		{
			for (int i = 0; i < _words.Length; i++)
			{
				Task.NavigateNext();
			}

		}

		[Test,Ignore("Can't be tested on task, make sure it is correct on view.")]
		public void GoingToNextWordSavesCurrentGloss()
		{
		}

		[Test]
		public void IsTaskCompleteTrueAtEnd()
		{
			Assert.IsFalse(Task.IsTaskComplete);
			NextToEnd();
			Assert.IsTrue(Task.IsTaskComplete);
			Task.NavigatePrevious();
			Assert.IsFalse(Task.IsTaskComplete);
		}
		[Test]
		public void CanBackupFromEnd()
		{
			Assert.IsFalse(Task.IsTaskComplete);
			NextToEnd();
			Task.NavigatePrevious();
			Assert.IsFalse(Task.IsTaskComplete);
			Assert.IsTrue(Task.CanNavigateNext);
		}

		[Test]
		public void NoWorkToDo()
		{
			AddEntryAndSense("one");
			AddEntryAndSense("two");
			AddEntryAndSense("three");
			Assert.IsTrue(Task.IsTaskComplete);
		}

		[Test]
		public void FirstWordAlreadyCollected()
		{
			 //add a word with the first wordlist-word already in a sense
			AddEntryAndSense("one");
		   Task.NavigateFirstToShow();
			Assert.AreEqual("two", Task.CurrentWord);
		}

		[Test]
		public void LastWordAlreadyCollected()
		{
			//add an entry with a sense using the last word in the list as a gloss
			AddEntryAndSense("three");

			Task.NavigateFirstToShow();
			Assert.AreEqual("one", Task.CurrentWord);
			Task.NavigateNext();
			Assert.IsTrue(Task.CanNavigateNext);
			Assert.AreEqual("two", Task.CurrentWord);
			Task.NavigateNext();
			Assert.IsTrue(Task.IsTaskComplete);//we don't get to see "three"
		}

		[Test]
		public void SkipMiddleWordAlreadyCollected()
		{
			AddEntryAndSense("two");
			Task.NavigateFirstToShow();

			Assert.AreEqual("one", Task.CurrentWord);
			Task.NavigateNext();
			Assert.AreEqual("three", Task.CurrentWord);
		}

		[Test]
		public void SkipFirstTwoWordsAlreadyCollected()
		{
			AddEntryAndSense("one");
			AddEntryAndSense("two");
			Task.NavigateFirstToShow();
			Assert.AreEqual("three", Task.CurrentWord);
		}

		[Test]
		public void AddWordNotInDB()
		{
			Task.NavigateFirstToShow();
			Assert.AreEqual(0, _lexEntryRepository.CountAllEntries());
			MultiText word = new MultiText();
			word[VernWs.Id] = "uno";
			Task.WordCollected(word);
			Assert.AreEqual(1, _lexEntryRepository.CountAllEntries());

		   //this is an attempt to get a failure that I was able to get at one time in the
			//app itself, but which I haven't got to fail under tests.  I believe I've
			//fixed the bug, but alas this never really demonstrated it.
			Assert.AreEqual(1, Task.GetMatchingRecords().Count);
		}

		[Test]
		public void AddWordAlreadyInDBAddsNewSense()
		{
			LexEntry e = _lexEntryRepository.CreateItem();
			e.LexicalForm[VernWs.Id] = "uno";
			Assert.AreEqual(1, _lexEntryRepository.CountAllEntries());
			MultiText word = new MultiText();
			word[VernWs.Id] = "uno";
			Assert.AreEqual(0, e.Senses.Count);
			_lexEntryRepository.SaveItem(e);

			Task.NavigateFirstToShow();
			Task.WordCollected(word);
			Assert.AreEqual(1, e.Senses.Count);
			Assert.AreEqual(1, _lexEntryRepository.CountAllEntries());
	   }


	   [Test]
	   public void AddWordASecondTime_DoesNothing()
	   {
		   RecordToken token = PrepareEntryWithOneGloss();
		   RecordToken token2 = PrepareEntryWithOneGloss();
		   Assert.AreSame(token, token2);
		   LexEntry entry = _lexEntryRepository.GetItem(token);
		   Assert.AreEqual(1, entry.Senses.Count);
		   Assert.AreEqual(1, _lexEntryRepository.CountAllEntries());
	   }

		private void AddEntryAndSense(string gloss)
		{
			LexEntry e = _lexEntryRepository.CreateItem();
			((LexSense) e.Senses.AddNew()).Gloss["en"] = gloss;
			_lexEntryRepository.SaveItem(e);
		}

		private GatherWordListTask Task
		{
			get
			{
				if (!_task.IsActive)
				{
					_task.Activate();
				}
				return ((GatherWordListTask) _task);
			}
		}

		/// <summary>
		/// test support for spell fixing
		/// </summary>
		[Test]
		public void RemovingGlossFromEmptyEntry_RemovesEntry()
		{
			RecordToken token = PrepareEntryWithOneGloss();

			//now simulate removing it, as when the user wants to correct spelling
			Task.TryToRemoveAssociationWithListWordFromEntry(token);
			Assert.AreEqual(0, _lexEntryRepository.GetEntriesWithMatchingLexicalForm("uno", VernWs).Count);
		}


		/// <summary>
		/// test support for spell fixing
		/// </summary>
		[Test]
		public void RemovingGlossFromEntryWithOtherSenses_OnlyRemovesGloss()
		{
			RecordToken token = PrepareEntryWithOneGloss();
			//now tweak the entry
			LexEntry entry = _lexEntryRepository.GetItem(token);
			LexSense leaveAloneSense = (LexSense)entry.Senses.AddNew();
			leaveAloneSense.Gloss.SetAlternative(_glossingLanguageWSId, "single");
			Assert.AreEqual(2, entry.Senses.Count);

			//now simulate removing it, as when the user wants to correct spelling
			Task.TryToRemoveAssociationWithListWordFromEntry(token);
			Assert.AreEqual(1, _lexEntryRepository.GetEntriesWithMatchingLexicalForm("uno", VernWs).Count);
			Assert.AreEqual(1, entry.Senses.Count);
		}


		/// <summary>
		/// test support for spell fixing
		/// </summary>
		[Test]
		public void RemovingAssociationWith_OnlyRemovesGloss()
		{
			RecordToken token = PrepareEntryWithOneGloss();
			//now tweak the entry
			LexEntry entry = _lexEntryRepository.GetItem(token);
			LexSense leaveAloneSense = (LexSense)entry.Senses.AddNew();
			leaveAloneSense.Gloss.SetAlternative(_glossingLanguageWSId, "single");
			Assert.AreEqual(2, entry.Senses.Count);

			//now simulate removing it, as when the user wants to correct spelling
			Task.TryToRemoveAssociationWithListWordFromEntry(token);
			Assert.AreEqual(1, _lexEntryRepository.GetEntriesWithMatchingLexicalForm("uno", VernWs).Count);
			Assert.AreEqual(1, entry.Senses.Count);
		}


		/// <summary>
		/// test support for spell fixing (ideally, this would move the sense, but this what we do for now)
		/// </summary>
		[Test]
		public void RemovingAssociationWhereSenseHasExample_DoesNothing()
		{
			RecordToken token = PrepareEntryWithOneGloss();
			//now tweak the entry
			LexEntry entry = _lexEntryRepository.GetItem(token);
			LexSense sense = (LexSense)entry.Senses[0];
			LexExampleSentence ex= (LexExampleSentence) sense.ExampleSentences.AddNew();
			ex.Sentence.SetAlternative(VernWs.Id, "blah blah");

			//now simulate removing it, as when the user wants to correct spelling
			Task.TryToRemoveAssociationWithListWordFromEntry(token);
			Assert.AreEqual(1, _lexEntryRepository.GetEntriesWithMatchingLexicalForm("uno", VernWs).Count);
			Assert.AreEqual(1, entry.Senses.Count);
			Assert.AreEqual("one", sense.Gloss.GetExactAlternative(_glossingLanguageWSId), "should not remove the gloss");
		}

		private RecordToken PrepareEntryWithOneGloss()
		{
			Task.NavigateAbsoluteFirst();
			MultiText word = new MultiText();

			word.SetAlternative(_vernacularLanguageWSId,"uno");
			Task.WordCollected(word);
			Assert.AreEqual(1, _lexEntryRepository.GetEntriesWithMatchingLexicalForm("uno", VernWs).Count);

			IList<RecordToken> entries = _lexEntryRepository.GetEntriesWithMatchingLexicalForm("uno", VernWs);

			return entries[0];
		}

		private WritingSystem VernWs
		{
			get
			{
				WritingSystem vernWs;
				BasilProject.Project.WritingSystems.TryGetValue(_vernacularLanguageWSId,out vernWs);
				return vernWs;
			}
		}

		[Test]
		public void WordCollected_AddSenseAppendsToExistingLexEntry()
		{
			string formToFind = "Bank";
			AddEntryWithLexemeForm("ignore1");
			LexEntry entryToMatch = AddEntryWithLexemeForm(formToFind);
			AddEntryWithLexemeForm("ignore2");

			MultiText lexemeForm = new MultiText();
			lexemeForm["en"] = formToFind;
			Assert.AreEqual(0, entryToMatch.Senses.Count);

			//add sense to empty entry
			LexSense sense = new LexSense();
			sense.Gloss["en"] = "money place";
			Db4oLexQueryHelper.AddSenseToLexicon(_lexEntryRepository, lexemeForm, sense);
			Assert.AreEqual(1, entryToMatch.Senses.Count);

			//add sense to  entry which already has one sense
			LexSense sense2 = new LexSense();
			Db4oLexQueryHelper.AddSenseToLexicon(_lexEntryRepository, lexemeForm, sense2);
			Assert.AreEqual(2, entryToMatch.Senses.Count);

			sense.Gloss["en"] = "side of river";
		}

		private LexEntry AddEntryWithLexemeForm(string lexemeForm)
		{
			LexEntry entry = _lexEntryRepository.CreateItem();
			entry.LexicalForm["en"] = lexemeForm;
			_lexEntryRepository.SaveItem(entry);
			return entry;
		}



	}

}