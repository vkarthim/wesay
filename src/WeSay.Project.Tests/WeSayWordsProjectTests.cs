using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using WeSay.Foundation;
using WeSay.Foundation.Progress;
using WeSay.Project;

namespace WeSay.Project.Tests
{
	[TestFixture]
	public class WeSayWordsProjectTests
	{
		private string _projectDirectory;

		[SetUp]
		public void Setup()
		{
			Reporting.ErrorReporter.OkToInteractWithUser = false;
			DirectoryInfo dirProject = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
			this._projectDirectory = dirProject.FullName;
		}

		[TearDown]
		public void TearDown()
		{
			Directory.Delete(this._projectDirectory, true);
		}


		[Test, Ignore()]
		public void MakeProjectFiles()
		{
			string path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			try
			{
				Directory.CreateDirectory(Directory.GetParent(path).FullName);
				WeSayWordsProject p = new WeSayWordsProject();
				p.CreateEmptyProjectFiles(path);
				Assert.IsTrue(Directory.Exists(path));
				Assert.IsTrue(Directory.Exists(p.PathToWeSaySpecificFilesDirectoryInProject));
			}
			finally
			{
				Directory.Delete(path, true);
			}
		}

		[Test, ExpectedException(typeof(ApplicationException))]
		public void WeSayDirNotInValidBasilDir()
		{
			string experimentDir = MakeDir(Path.GetTempPath(), Path.GetRandomFileName());
			string weSayDir = MakeDir(experimentDir, "WeSay");
			string wordsPath = Path.Combine(weSayDir, "AAA.words");
			File.Create(wordsPath).Close();
			TryLoading(wordsPath, experimentDir);
		}

		[Test]
		public void LoadPartsOfSpeechList()
		{
			WeSayWordsProject p = CreateAndLoad(false);
			OptionsList list = p.GetOptionsList("PartsOfSpeech.xml");
			Assert.IsTrue(list.Options.Count>2);
		}

		[Test]
		public void CorrectFieldToOptionListNameDictionary()
		{
			WeSayWordsProject p = CreateAndLoad(false);
			OptionsList list = p.GetOptionsList("PartsOfSpeech.xml");
			Dictionary<string, string> dict = p.GetFieldToOptionListNameDictionary();
			Assert.AreEqual("PartsOfSpeech", dict[LexicalModel.LexSense.WellKnownProperties.PartOfSpeech]);
		}


		private static WeSayWordsProject CreateAndLoad(bool doMakeFileMatchingProjectName)
		{
			string experimentDir = MakeDir(Path.GetTempPath(), Path.GetRandomFileName());
			string projectDir = MakeDir(experimentDir, "TestProj");
			string weSayDir = MakeDir(projectDir, "WeSay");
//            MakeDummyWordsFile("AAA.words", weSayDir);
//            if (doMakeFileMatchingProjectName)
//            {
//                MakeDummyWordsFile("TestProj.words", weSayDir);
//            }
//            MakeDummyWordsFile("ZZZ.words", weSayDir);
			WeSayWordsProject p = new WeSayWordsProject();
			p.LoadFromProjectDirectoryPath(projectDir);
			return p;
		}



		private static void MakeDummyWordsFile(string fileName, string weSayDir)
		{
			File.Create(Path.Combine(weSayDir, fileName)).Close();
		}

		private static string MakeDir(string existingParent, string newChild)
		{
			string dir = Path.Combine(existingParent,newChild);
			Directory.CreateDirectory(dir);
			return dir;
		}

		private static bool TryLoading(string lexiconPath, string experimentDir)
		{
			try
			{
				WeSayWordsProject p = new WeSayWordsProject();
				return p.LoadFromLiftLexiconPath(lexiconPath);
			}
			finally
			{
				Directory.Delete(experimentDir, true);
			}
		}
	}
}