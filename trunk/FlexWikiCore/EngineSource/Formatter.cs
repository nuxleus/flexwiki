#region License Statement
// Copyright (c) Microsoft Corporation.  All rights reserved.
//
// The use and distribution terms for this software are covered by the 
// Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file CPL.TXT at the root of this distribution.
// By using this software in any fashion, you are agreeing to be bound by 
// the terms of this license.
//
// You must not remove this notice, or any other, from this software.
#endregion

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using FlexWiki;


namespace FlexWiki.Formatting
{
	/// <summary>
	/// Formatter is the core engine that understands wiki text and converts it to some output language (e.g., HTML).  
	/// </summary>
	/// <remarks>
	/// The output language should be completely independent of the formatter (since Formatter uses a pluggable formatting object
	/// called a WikiOutput to output actual language constructs (e.g., HTML, etc.).  In practice, this isloation is only about 90% complete 
	/// and should get finished up by some happy soul (perhaps the first person who tries to generate XAML :-))..
	/// 
	/// A Formatter is quite stateful and a new instance is used for each translation of some input to output.  Instance are not reused.
	/// </remarks>
	public class Formatter : IWikiToPresentation
	{

		#region FormattedString
		/// <summary>
		/// Format a string of wiki content in a given OutputFormat with references all being relative to a given namespace
		/// </summary>
		/// <param name="topic">Topic context</param>
		/// <param name="input">The input wiki text</param>
		/// <param name="format">The desired output format</param>
		/// <param name="relativeToContentBase">References will be relative to this namespace</param>
		/// <param name="lm">Link maker (unless no relativeTo content base is provide)</param>
		/// <param name="accumulator">composite cache rule in which to accumulate cache rules</param>
		/// <returns></returns>
		public static string FormattedString(AbsoluteTopicName topic, string input, OutputFormat format, ContentBase relativeToContentBase, LinkMaker lm, CompositeCacheRule accumulator)
		{
			// TODO -- some of the cases in which this call happens actually *are* nested, even though the false arg
			// below says that they aren't.  This causes scripts to be emitted multiple times -- should clean up.
			WikiOutput output = WikiOutput.ForFormat(format, null);	
			Hashtable ew;
      if (relativeToContentBase != null)
      {
        ew = relativeToContentBase.ExternalWikiHash();
      }
      else
      {
        ew = new Hashtable();
      }
			Format(topic, input, output, relativeToContentBase, lm, ew, 0, accumulator);
			return output.ToString();
		}
		#endregion

		#region FormattedTopic
		/// <summary>
		/// Answer the formatted text for a given topic, formatted using a given OutputFormat and possibly showing diffs with the previous revision
		/// </summary>
		/// <param name="topic">The topic</param>
		/// <param name="format">What format</param>
		/// <param name="showDiffs">true to show diffs</param>
		/// <param name="accumulator">composite cache rule in which to accumulate cache rules</param>
		/// <returns></returns>
		public static string FormattedTopic(AbsoluteTopicName topic, OutputFormat format, AbsoluteTopicName previousVersion, Federation aFederation, LinkMaker lm, CompositeCacheRule accumulator)
		{ 
			ContentBase relativeToBase = aFederation.ContentBaseForNamespace(topic.Namespace);
			return FormattedTopicWithSpecificDiffs(topic, format, previousVersion, aFederation, lm, accumulator);
		}
		#endregion
 
		#region FormattedTopicWithSpecificDiffs
		/// <summary>
		/// Answer the formatted text for a given topic, formatted using a given OutputFormat and possibly showing diffs
		/// with a specified revision
		/// </summary>
		/// <param name="topic">The topic</param>
		/// <param name="format">What format</param>
		/// <param name="showDiffs">true to show diffs</param>
		/// <param name="accumulator">composite cache rule in which to accumulate cache rules (ignored for diffs)</param>
		/// <returns></returns>
		public static string FormattedTopicWithSpecificDiffs(AbsoluteTopicName topic, OutputFormat format, AbsoluteTopicName diffWithThisVersion, Federation aFederation, LinkMaker lm, CompositeCacheRule accumulator)
		{ 

			// Setup a special link maker that knows what to make the edit links return to 
			LinkMaker linker = new LinkMaker(lm.SiteURL());
			linker.ReturnToTopicForEditLinks = topic;
			ContentBase relativeToBase = aFederation.ContentBaseForNamespace(topic.Namespace);

			if (accumulator != null)
				accumulator.Add(relativeToBase.CacheRuleForAllPossibleInstancesOfTopic(topic));
			WikiOutput output = WikiOutput.ForFormat(format, null);
			if (diffWithThisVersion != null)
			{
				ArrayList styledLines = new ArrayList();
				IList leftLines;
				IList rightLines;
				using (TextReader srLeft = relativeToBase.TextReaderForTopic(topic.LocalName))
				{
					leftLines = MergeBehaviorLines(srLeft.ReadToEnd().Replace("\r", "").Split('\n'));
				}
				using (TextReader srRight = relativeToBase.TextReaderForTopic(diffWithThisVersion.LocalName))
				{
					rightLines = MergeBehaviorLines(srRight.ReadToEnd().Replace("\r", "").Split('\n'));
				}
				IEnumerable diffs = Diff.Compare(leftLines, rightLines);
				foreach (LineData ld in diffs)
				{ 
					LineStyle style = LineStyle.Unchanged;
					switch (ld.Type)
					{
						case LineType.Common:
							style = LineStyle.Unchanged;
							break;

						case LineType.LeftOnly:
							style = LineStyle.Add;
							break;

						case LineType.RightOnly:
							style = LineStyle.Delete;
							break;
					}
					styledLines.Add(new StyledLine(ld.Text, style));
				}
				Format(topic, styledLines, output, relativeToBase, linker, relativeToBase.ExternalWikiHash(), 0, accumulator);
			}
			else
			{
				using (TextReader sr = relativeToBase.TextReaderForTopic(topic.LocalName))
				{
					Format(topic, sr.ReadToEnd(), output, relativeToBase, linker, relativeToBase.ExternalWikiHash(), 0, accumulator);
				}
			}

			return output.ToString();
		}
		#endregion
		
		/// <summary>
		/// The source data for formatting: a list of StyledLines 
		/// </summary>
		private IList								_Source; 
		/// <summary>
		/// The output is being sent out using this object.
		/// </summary>
		private WikiOutput							_Output;
		/// <summary>
		/// An object that is used to make links
		/// </summary>
		private LinkMaker							_LinkMaker;
		/// <summary>
		/// The content base that the source content resides in
		/// </summary>
		private ContentBase							_ContentBase;

		/// <summary>
		/// Topic name for which we're doing formatting (if known; can be null)
		/// </summary>
		private AbsoluteTopicName				_Topic;

		private AbsoluteTopicName				CurrentTopic
		{
			get
			{
				return _Topic;
			}
		}

		#region CurrentTopicInfo
		private TopicInfo	_CurrentTopicInfo;
		private TopicInfo  CurrentTopicInfo
		{
			get
			{
				if (_CurrentTopicInfo != null)	
					return _CurrentTopicInfo;
				if (CurrentTopic == null)
					return null;
				_CurrentTopicInfo = new TopicInfo(TheFederation, CurrentTopic);
				return _CurrentTopicInfo;
			}
		}
		#endregion

		#region ContentBase
		/// <summary>
		///  The content base that the source content resides in
		/// </summary>
		public ContentBase ContentBase
		{
			get
			{
				return _ContentBase;
			}
			set
			{
				_ContentBase = value;
			}
		}
		#endregion

		/// <summary>
		/// The current line number
		/// </summary>
		private int										_CurrentLineIndex;

		/// <summary>
		/// The map to external Wikis that is generated by locally-defined external Wiki map entries
		/// </summary>
		private Hashtable								_ExternalWikiMap;

		/// <summary>
		/// The relative base for heading nesting (normally 0, but can be higher for nested topic include)
		/// </summary>
		private int										_HeadingLevelBase;

		#region Format
		/// <summary>
		/// Format a given input string to the given output
		/// </summary>
		/// <param name="topic">Topic context</param>
		/// <param name="source">Input wiki string</param>
		/// <param name="output">Output object to which output is sent</param>
		/// <param name="contentBase">The ContentBase that contains the wiki string text</param>
		/// <param name="maker">A link maker </param>
		/// <param name="external">External wiki map</param>
		/// <param name="headingLevelBase">Relative heading level</param>
		/// <param name="accumulator">composite cache rule in which to accumulate cache rules</param>
		static public void Format(AbsoluteTopicName topic, string source, WikiOutput output, ContentBase contentBase, LinkMaker maker, Hashtable external, int headingLevelBase, CompositeCacheRule accumulator)
		{
			Formatter f = new Formatter(topic, source, output, contentBase, maker, external,  headingLevelBase, accumulator);
			f.Format();
		}

		/// <summary>
		/// Format a given input string to the given output
		/// </summary>
		/// <param name="source">Input wiki content as a list of StyledLines</param>
		/// <param name="output">Output object to which output is sent</param>
		/// <param name="contentBase">The ContentBase that contains the wiki string text</param>
		/// <param name="maker">A link maker </param>
		/// <param name="external">External wiki map</param>
		/// <param name="headingLevelBase">Relative heading level</param>
		/// <param name="accumulator">composite cache rule in which to accumulate cache rules</param>
		/// 
		static public void Format(AbsoluteTopicName topic, IList source, WikiOutput output, ContentBase contentBase, LinkMaker maker, Hashtable external, int headingLevelBase, CompositeCacheRule accumulator)
		{
			Formatter f = new Formatter(topic, source, output, contentBase, maker, external, headingLevelBase, accumulator);
			f.Format();
		}
		#endregion

		#region WikiToPresentation
		static public IWikiToPresentation WikiToPresentation(AbsoluteTopicName topic, WikiOutput output, ContentBase contentBase, LinkMaker maker, Hashtable external, int headingLevelBase, CompositeCacheRule accumulator)
		{
			ArrayList lines = new ArrayList();
			lines.Add(new StyledLine("", LineStyle.Unchanged));
			return new Formatter(topic, lines, output, contentBase, maker, external,  headingLevelBase, accumulator);
		}
		#endregion

		#region Ctor
		/// <summary>
		/// Create a new formatter for a string of input content.
		/// </summary>
		/// <param name="source">Input wiki string</param>
		/// <param name="output">Output object to which output is sent</param>
		/// <param name="contentBase">The ContentBase that contains the wiki string text</param>
		/// <param name="maker">A link maker </param>
		/// <param name="external">External wiki map</param>
		/// <param name="headingLevelBase">Relative heading level</param>
		/// <param name="accumulator">composite cache rule in which to accumulate cache rules</param>
		/// 
		Formatter(AbsoluteTopicName topic, string source, WikiOutput output, ContentBase contentBase, LinkMaker maker, Hashtable external, int headingLevelBase, CompositeCacheRule accumulator)
		{
			_Topic = topic;
			_Source = SplitStringIntoStyledLines(source, LineStyle.Unchanged);
			_LinkMaker = maker;	
			ContentBase = contentBase;
			_Output = output;
			_ExternalWikiMap = external;
			_HeadingLevelBase = headingLevelBase;
			_CacheRuleAccumulator = accumulator;
		}

		/// <summary>
		/// Create a new formatter for an input list of StyledLines
		/// </summary>
		/// <param name="source">Input wiki content as a list of StyledLines</param>
		/// <param name="output">Output object to which output is sent</param>
		/// <param name="contentBase">The ContentBase that contains the wiki string text</param>
		/// <param name="maker">A link maker </param>
		/// <param name="external">External wiki map</param>
		/// <param name="headingLevelBase">Relative heading level</param>
		/// <param name="accumulator">composite cache rule in which to accumulate cache rules</param>
		/// 
		Formatter(AbsoluteTopicName topic, IList source, WikiOutput output, ContentBase contentBase, LinkMaker maker, Hashtable external, int headingLevelBase, CompositeCacheRule accumulator)
		{
			_Topic = topic;
			_Source = source;
			_Output = output;
			_LinkMaker = maker;
			ContentBase = contentBase;
			_ExternalWikiMap = external;
			_HeadingLevelBase = headingLevelBase;
			_CacheRuleAccumulator = accumulator;
		}
		#endregion

		#region CacheRuleAccumulator
		private CompositeCacheRule _CacheRuleAccumulator;
		private CompositeCacheRule CacheRuleAccumulator
		{
			get
			{
				return _CacheRuleAccumulator;
			}
		}
		#endregion

		#region AddCacheRule
		/// <summary>
		/// Add a cache rule to the cache rule accumulator (or ignore if we aren't accumulating)
		/// </summary>
		/// <param name="aRule"></param>
		void AddCacheRule(CacheRule aRule)
		{
			if (CacheRuleAccumulator != null)
				CacheRuleAccumulator.Add(aRule);
		}
		#endregion

		/// <summary>
		/// string pattern for what comes before a wiki name
		/// </summary>
		public static string beforeWikiName = "(?<before>^|\\||\\*|'|\\s|\\(|\\{|\\!)";
		/// <summary>
		/// string pattern for what comes after a wiki name
		/// </summary>
		public static string afterWikiName = "(?<after>'|\\||\\s|@|$|\\.|,|:|'|;|\\}|\\?|_|\\)|\\!)";
		/// <summary>
		/// string pattern for the optional anchor after a wiki name.
		/// </summary>
		public static string wikiNameAnchor = @"(?:\#)?(?<anchor>([\w\d]+))?";

		/// <summary>
		/// Unicode compatible regex fragments.
		/// 
		/// Lu: Letter Uppercase
		/// Ll: Letter Lowercase
		/// Lt: Letter Title
		/// Lo: Letter Other
		/// Nd: Number decimal
		/// Pc: Punctation connector
		/// </summary>
		public static string AZ = "[\\p{Lu}]"; // A-Z
		public static string az09 = "[\\p{Ll}\\p{Lt}\\p{Lo}\\p{Nd}]"; // a-z0-9
		public static string Az09 = "[\\p{Lu}\\p{Ll}\\p{Lt}\\p{Lo}\\p{Nd}]"; // A-Za-z0-9


		/// <summary>
		/// string pattern for legal namespace names
		/// </summary>
		private static string namespaceName = AZ + "[\\w]+";

		private static string startsWithMulticaps = "(" + AZ + "{2,}" + az09 + "+" + Az09 + "*)";
		private static string startsWithOneCap = "(" + AZ + az09 + "+" + Az09 + "*" + AZ + "+" + Az09 + "*)";
		private static string unbracketedWikiName = "(?:_?" + startsWithMulticaps + "|" + startsWithOneCap + ")";

		private static string bracketedWikiName = "\\[(?:[\\w ]+)\\]"; // \w is a word character (A-z0-9_)
		private static string unqualifiedWikiName = "(?:" + "(?:" + unbracketedWikiName + ")|(?:" + bracketedWikiName + ")" + ")";
		private static string qualifiedWikiName = "(?:" + namespaceName + "\\.)*"  + unqualifiedWikiName;
		private static string forcedLocalWikiName = "\\."  + unqualifiedWikiName;
		public static string wikiName = "(?:" + "(?:" + qualifiedWikiName + ")|(?:" + unqualifiedWikiName + ")|(?:" + forcedLocalWikiName + ")" + ")";
		private static string relabelPrefix = "(\"(?<relabel>[^\"]+)\"\\:)";
		
		// A wiki name is a topic name, possibly with a namespace
		public static string extractWikiNamesString = beforeWikiName + "(?<topic>" + wikiName + ")"  + afterWikiName;
		public static Regex extractWikiNames = new Regex(extractWikiNamesString) ;

		// A wiki link is a wiki name, possible prefixed by a relabel
		private static string beforeOrRelabel = "(" + relabelPrefix + "|" + beforeWikiName + ")";
		public static string extractWikiLinksString = beforeOrRelabel + "(?<topic>" + wikiName + ")"  + wikiNameAnchor + afterWikiName;
		public static Regex extractWikiLinks = new Regex(extractWikiLinksString) ;

		private static string emailAddressString = @"([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)";
		private static Regex emailAddress = new Regex(emailAddressString);

		#region Output
		/// <summary>
		/// Answer the object to which output is being written
		/// </summary>
		public WikiOutput Output
		{
			get
			{
				return _Output;
			}
		}
		#endregion

		#region SplitStringIntoStyledLines
		private static IList SplitStringIntoStyledLines(string input, LineStyle aStyle)
		{
			string sp;
			ArrayList answer = new ArrayList();
			
			sp = input.TrimEnd(new char[]{'\n'});
			sp = sp.Replace("\r", "");
			sp = sp.TrimEnd(new char[]{'\n'});
			foreach (string s in sp.Split (new char[]{'\n'}) )
				answer.Add(new StyledLine(s, aStyle));
			return MergeStyledBehaviorLines(answer);
		}
		#endregion

		#region MergeStyledBehaviorLines
		/// <summary>
		/// Take a list of styled lines as input and produce a new list where contiguous lines representing a behavior expression that spans lines
		/// have been combined into a single line.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		private static IList MergeStyledBehaviorLines(IList input)
		{
			ArrayList answer = new ArrayList();
			// We do a fancy thing here.  If there is an unterminated behavior that starts on a line, we suck up all following 
			// lines until we get the one that has the closing terminator.  We then combine them into one.
			//
			// Subtlties:
			//   - We upgrade any line styles, so that the new compound line gets the highest priority style found across all the lines

			for (int lineNumber = 0; lineNumber < input.Count; lineNumber++)
			{
				StyledLine eachLine = (StyledLine)(input[lineNumber]);
				int thisLineDelimiterCount = CountOccurrences(eachLine.Text, BehaviorDelimiter);
				if ((thisLineDelimiterCount  % 2) != 0)
				{
					while (true)
					{
						lineNumber++;
						if (lineNumber >= input.Count)
							break;
						StyledLine nextLine = (StyledLine)(input[lineNumber]);
						eachLine = eachLine.CombinedWith(nextLine);
						if (nextLine.Text.IndexOf(BehaviorDelimiter) != -1)
							break;
					}
				}
				answer.Add(eachLine);
			}
			return answer;
		}
		#endregion

		#region MergeBehaviorLines
		/// <summary>
		/// Take a list of lines of text as input and produce a new list where contiguous lines representing a behavior expression that spans lines
		/// have been combined into a single line.
		/// </summary>
		/// <param name="input">IList of strings</param>
		/// <returns></returns>
		private static IList MergeBehaviorLines(IList input)
		{
			ArrayList answer = new ArrayList();
			for (int lineNumber = 0; lineNumber < input.Count; lineNumber++)
			{
				string eachLine = (string)(input[lineNumber]);
				int thisLineDelimiterCount = CountOccurrences(eachLine, BehaviorDelimiter);
				if ((thisLineDelimiterCount  % 2) != 0)
				{
					while (true)
					{
						lineNumber++;
						if (lineNumber >= input.Count)
							break;
						string nextLine = (string)(input[lineNumber]);
						eachLine = eachLine + Environment.NewLine + nextLine;
						if (nextLine.IndexOf(BehaviorDelimiter) != -1)
							break;
					}
				}
				answer.Add(eachLine);
			}
			return answer;
		}
		#endregion


		#region LinkMaker
		private LinkMaker LinkMaker()
		{
			return _LinkMaker;
		}
		#endregion

		#region IsNextLinePre
		/// <summary>
		/// Answer true if the next line is pre-formatted
		/// </summary>
		/// <returns></returns>
		private bool IsNextLinePre()
		{
			if (_CurrentLineIndex + 1 <  _Source.Count)
			{
				StyledLine line = (StyledLine)_Source[_CurrentLineIndex+1];
				string next = line.Text;
				return next.StartsWith(" ") || Regex.IsMatch(next, "^[ \t]+[^ \t*1]");
			}        
			return false;
		}
		#endregion

		#region CloseLists
		/// <summary>
		/// Close out any open lists and decrement associated counters
		/// </summary>
		/// <param name="listNest"></param>
		/// <param name="olistNest"></param>
		private void CloseLists(ref int listNest, ref int olistNest)
		{
			int jj;
			// if we're in a list, close all the <ul>s
			for (jj = 0; jj < listNest; jj++)
			{
				_Output.WriteCloseUnorderedList();
				listNest--;
			}
			for (jj = 0; jj < olistNest; jj++)
			{
				_Output.WriteCloseOrderedList();
				olistNest--;
			}
		}
		#endregion


		#region private state classes
		#region class State
		/// <summary>
		/// Abstract superclass for the various states that the state machine can be in
		/// </summary>
		private abstract class State
		{
			Formatter _Formatter;

			public State(Formatter f)
			{
				_Formatter = f;
			}

			virtual public void Exit()
			{
			}

			virtual public void Enter()
			{
			}

			protected WikiOutput Output
			{
				get
				{
					return _Formatter.Output;
				}
			}

		}
		#endregion
		
		#region class TableState
		class TableState : State
		{
			public TableState(Formatter f) : base (f)
			{
			}

			override public void Exit()
			{
				Output.WriteCloseTable();
			}

			override public void Enter()
			{
			}

			public bool HasBorder;

		}
		#endregion

		#region class PreState
		class PreState : State
		{
			public PreState(Formatter f) : base (f)
			{
			}

			override public void Exit()
			{
				Output.WriteClosePreformatted();
			}

			override public void Enter()
			{
				Output.WriteOpenPreformatted();
			}
		}
		#endregion
	
		#region class ListState
		abstract class ListState : State
		{
			protected int listNest = 0;

			abstract public void WriteIndent();
			abstract public void WriteOutdent();

			public ListState(Formatter f) : base (f)
			{
			}

			public void SetNesting(int thisNest)
			{
				if (listNest < thisNest)
				{
					while (listNest < thisNest)
					{
						WriteIndent();
					}
				}
				else if (listNest > thisNest)
				{
					while (listNest > thisNest)
					{
						WriteOutdent();
					}
				}
			}
		}
		#endregion

		#region class UnorderedListState
		class UnorderedListState : ListState
		{
			public UnorderedListState(Formatter f) : base (f)
			{
			}

			override public void WriteIndent()
			{
				Output.WriteOpenUnorderedList();
				listNest++;
			}

			override public void WriteOutdent()
			{
				Output.WriteCloseUnorderedList();
				listNest--;
			}

			override public void Exit()
			{
				SetNesting(0);
			}

			override public void Enter()
			{
				SetNesting(1);
			}
		}
		#endregion 

		#region class OrderedListState
		class OrderedListState : ListState
		{
			public OrderedListState(Formatter f) : base (f)
			{
			}

			override public void WriteIndent()
			{
				Output.WriteOpenOrderedList();
				listNest++;
			}

			override public void WriteOutdent()
			{
				Output.WriteCloseOrderedList();
				listNest--;
			}

			override public void Exit()
			{
				SetNesting(0);
			}

			override public void Enter()
			{
				SetNesting(1);
			}

		}
		#endregion 

		#region class NeutralState
		class NeutralState : State
		{
			public NeutralState(Formatter f) : base (f)
			{
			}
		}
		#endregion 

		#endregion 
				
		#region CurrentState
		private State _CurrentState;

		/// <summary>
		/// Get and set the current state of the state machine; when transitioning, make sure state.Enter() and state.Exit() get called
		/// </summary>
		private State CurrentState
		{
			get
			{
				return _CurrentState;
			}
			set
			{
				if (_CurrentState != null)
					_CurrentState.Exit();
				_CurrentState = value;
				if (_CurrentState != null)
					_CurrentState.Enter();
			}
		}
		#endregion 

		#region Ensure
		/// <summary>
		/// Make sure we're in a particular state and if we aren't create that state and enter it
		/// </summary>
		/// <param name="aType"></param>
		private void Ensure(Type aType)
		{
			if (CurrentState != null && CurrentState.GetType() == aType)
				return;	// already what's requested
			CurrentState = (State)(Activator.CreateInstance(aType, new object[]{this}));	
		}
		#endregion 

		#region CountOccurrences
		private static int CountOccurrences(string lookIn, string lookFor)
		{
			int answer = 0;
			int offset = 0;
			while (true)
			{
				int found = lookIn.IndexOf(lookFor, offset);
				if (found == -1)
					break;
				offset = found + lookFor.Length;
				answer++;
			}
			return answer;
		}
		#endregion

		#region IsBeyondSafeNestingDepth
		private const int MaxiumuNestingDepth = 20;

		public bool IsBeyondSafeNestingDepth
		{
			get
			{
				return _Output.GetNestingLevel() > MaxiumuNestingDepth;		// No more nesting that N deep
			}
		}
		#endregion

		#region Format
		/// <summary>
		/// Main formatting function.  The process of formatting the input to the output starts here.
		/// </summary>
		public void Format()
		{
			if (Federation.GetPerformanceCounter(Federation.PerformanceCounterNames.TopicFormat) != null)
				Federation.GetPerformanceCounter(Federation.PerformanceCounterNames.TopicFormat).Increment();


			CurrentState = new NeutralState(this);
			_CurrentLineIndex = 0;
			bool inMultilineProperty = false;
			bool currentMultilinePropertyIsHidden = false;
			string multiLinePropertyDelim = null;
			bool inPreBlock=false;
			string preBlockKey=null;

			_Output.Begin();

			for (int lineNumber = 0; lineNumber < _Source.Count; lineNumber++)
			{
				StyledLine eachLine = (StyledLine)(_Source[lineNumber]);
				string each = eachLine.Text;
				_Output.Style = eachLine.Style;

				each = StripHTMLSpecialCharacters(each);

				if ( inPreBlock )
				{
					if ( each.StartsWith("}@") && each.Substring(2).Trim() == preBlockKey )
					{
						Ensure(typeof(NeutralState));
						inPreBlock = false;
						preBlockKey = null;
					}
					else
					{
						if (false == currentMultilinePropertyIsHidden)
						{
							_Output.Write(Regex.Replace(each, "\t", "        "));
							_Output.WriteEndLine();
						}
						_CurrentLineIndex++;
					}
					continue;
				}
				else if ( !inMultilineProperty && each.StartsWith("{@") )
				{
					Ensure(typeof(PreState));
					inPreBlock = true;
					preBlockKey = each.Substring(2).Trim();
					continue;
				}

				// Make all the 8-space sequences into tabs
				each = Regex.Replace(each, " {8}", "\t");
    
				// See if this is the first line of a multiline property.
				if (!inMultilineProperty && ContentBase.MultilinePropertyRegex.IsMatch(each))
				{
					// OK, here we go -- time to output the header
					Match m = ContentBase.MultilinePropertyRegex.Match(each);
					string name = m.Groups["name"].Value;
					string val = m.Groups["val"].Value;
					string leader = m.Groups["leader"].Value;
					string delim = m.Groups["delim"].Value;
					multiLinePropertyDelim = ContentBase.ClosingDelimiterForOpeningMultilinePropertyDelimiter(delim);
					currentMultilinePropertyIsHidden = leader == ":";

					// Write out the anchor field.
					_Output.WriteOpenAnchor(name);
					if (!currentMultilinePropertyIsHidden)	// Don't bother showing out hidden page properties
					{
						val = val.Trim();

						// Do the normal processing
						name = StripHTMLSpecialCharacters(name);
						name = ProcessLineElements(name);
						val = StripHTMLSpecialCharacters(val);
						val = ProcessLineElements(val);

						_Output.WriteOpenProperty(name);
						if (ContentBase.IsBehaviorPropertyDelimiter(delim))
							_Output.Write(delim);
						_Output.Write(val);
					}
					inMultilineProperty = true;
					continue;
				}

				if (inMultilineProperty)
				{
					if (each.StartsWith(multiLinePropertyDelim))
					{
						// We're done!
						if (!currentMultilinePropertyIsHidden)
						{
							if (ContentBase.IsBehaviorPropertyDelimiter(multiLinePropertyDelim))
							{
								_Output.Write(multiLinePropertyDelim);
							}
							// Make sure we close off things like tables before we close the property.
							Ensure(typeof(NeutralState));
							_Output.WriteCloseProperty();
						}
						_Output.WriteCloseAnchor();
						inMultilineProperty = false;
						currentMultilinePropertyIsHidden = false;
						continue;
					}

					// If it's a line for a property behavior, just show it -- don't perform any processing
					if (ContentBase.IsBehaviorPropertyDelimiter(multiLinePropertyDelim))
					{
						if (!currentMultilinePropertyIsHidden)
						{
							_Output.WriteSingleLine(each);
						}
						continue;
					}
				}


				if (Formatter.StripExternalWikiDef(_ExternalWikiMap, each)) 
					continue;
        
				// empty line resets everything (except pre and multiline properties )
				if (each.Trim().Length == 0)
				{
					if (!(CurrentState is PreState) || (CurrentState is PreState && !IsNextLinePre()))
						Ensure(typeof(NeutralState));
					_Output.WriteEndLine();
				}
				else if ((each.StartsWith("----")) && (false == currentMultilinePropertyIsHidden))
				{
					Ensure(typeof(NeutralState));
					_Output.WriteRule();
				}
					// insert topic -- {{IncludeSomeTopic}} ?
				else if ((!each.StartsWith(" ") & Regex.IsMatch(each, @"^[\t]*\{\{" + wikiName + @"\}\}[\s]*$")) && 
					(false == currentMultilinePropertyIsHidden))
				{
					Regex nameGetter = new Regex("(?<topic>" + wikiName + ")");
					string name = nameGetter.Matches(each)[0].Groups["topic"].Value;
					RelativeTopicName topicName = new RelativeTopicName(name);

					// Count the tabs
					int tabs = 0;
					string tabber = each;
					while (tabber.Length > 0 && (tabber.StartsWith("\t")))
					{
						tabs++;
						tabber = tabber.Substring(1);
					}

					Ensure(typeof(NeutralState));
					AddCacheRule(ContentBase.CacheRuleForAllPossibleInstancesOfTopic(topicName));
					if (ContentBase.TopicExists(topicName)) 
					{
						if ((!IsBeyondSafeNestingDepth) && (false == currentMultilinePropertyIsHidden))
						{
							_Output.Write(IncludedTopic(topicName, _HeadingLevelBase + tabs));
						}
					}
					else
					{
						EnsureParaOpen();
						_Output.Write(LinkWikiNames(each));
						EnsureParaClose();
					}
				}
					// line begins with a space, it's PRE time!
				else if ((each.StartsWith(" ") || Regex.IsMatch(each, "^[ \t]+[^ \t*1]")) && 
					(false == currentMultilinePropertyIsHidden))
				{
					Ensure(typeof(PreState));
					_Output.Write(Regex.Replace(each, "\t", "        "));
				}

				else
				{
					// OK, it's likely more complicated

					// Continue if we're inside a multiline hidden property.
					if (true == currentMultilinePropertyIsHidden)
					{
						continue;
					}

					// See if this is a bullet line
					if (each.StartsWith("\t"))
					{
						each = ProcessLineElements(each);
						// Starts with a tab - might be a list (we'll see)
						// Count the tabs
						int thisNest = 0;
						while (each.Length > 0 && (each.StartsWith("\t")))
						{
							thisNest++;
							each = each.Substring(1);
						}

						if (each.StartsWith("*"))
						{
							each = each.Substring(1);
							// We're in a list - make sure we've got the right <ul> nesting setup
							// Could need more or fewer
							Ensure(typeof(UnorderedListState));
							((UnorderedListState)(CurrentState)).SetNesting(thisNest);
							_Output.WriteListItem(each);
						}
						else if (each.StartsWith("1."))
						{
							each = each.Substring(2);
							Ensure(typeof(OrderedListState));
							((OrderedListState)(CurrentState)).SetNesting(thisNest);
							_Output.WriteListItem(each);
						}
						else
						{
							// False alarm (just some tabs)
							_Output.Write(Regex.Replace(each, "\t", "        "));
						}
					}
					else if (each.StartsWith("||") && each.EndsWith("||") && each.Length >= 4)
					{
						bool firstRow = !(CurrentState is TableState);
						Ensure(typeof(TableState));
						TableState ts = (TableState)CurrentState;

						string endless = each.Substring(2, each.Length - 4);

						// Write the row
						bool firstCell = true;
						foreach (string eachCell in Regex.Split(endless, @"\|\|"))
						{
							string cellContent = eachCell;
							// Check the cell for formatting
							TableCellInfo info = new TableCellInfo();
							if (cellContent.StartsWith("{"))
							{
								int end = cellContent.IndexOf("}", 1);
								if (end != -1)
								{
									string fmt = cellContent.Substring(1, end - 1);
									cellContent = cellContent.Substring(end + 1);
									string result = info.Parse(fmt);
									if (result != null)
										cellContent = "(Error: " + result + ") " + cellContent;
								}
							}
							if (firstCell)
							{
								if (firstRow)
								{
									ts.HasBorder = info.HasBorder;
									Output.WriteOpenTable(info.TableAlignment, info.HasBorder, info.TableWidth);
								}
								_Output.WriteOpenTableRow();
							}
							_Output.WriteTableCell(ProcessLineElements(cellContent), info.IsHighlighted, info.CellAlignment, info.ColSpan, info.RowSpan, ts.HasBorder, info.AllowBreaks, info.CellWidth);
							firstCell = false;
						}
						_Output.WriteCloseTableRow();
					}
					else
					{
						Ensure(typeof(NeutralState));

						// See if we've got a heading prefix
						int heading = -1;
						if (each.StartsWith("!!!!!!!"))
							heading = 7;
						else if (each.StartsWith("!!!!!!"))
							heading = 6;
						else if (each.StartsWith("!!!!!"))
							heading = 5;
						else if (each.StartsWith("!!!!"))
							heading = 4;
						else if (each.StartsWith("!!!"))
							heading = 3;
						else if (each.StartsWith("!!"))
							heading = 2;
						else if (each.StartsWith("!"))
							heading = 1;

						if (heading != -1)
						{
							_Output.WriteHeading(ProcessLineElements(each.Substring(heading)), _HeadingLevelBase + heading);
						}
						else
						{
							// If it's a single-line field, wrap it visually
							if (ContentBase.PropertyRegex.IsMatch(each))
							{
								Match m = ContentBase.PropertyRegex.Match(each);
								string name = m.Groups["name"].Value;
								string val = m.Groups["val"].Value;
								string leader = m.Groups["leader"].Value;
								bool isLeader = leader == ":";

								// Write out an anchor tag.
								_Output.WriteOpenAnchor(name);
								if (!isLeader)	// Don't bother showing out hidden page properties
								{
									// Do the normal processing
									name = ProcessLineElements(name);

									val = val.Trim();
									val = ProcessLineElements(val);

									_Output.WriteOpenProperty(name);
									_Output.Write(val);
									_Output.WriteCloseProperty();
								}
								_Output.WriteCloseAnchor();
							}
							else
							{
								// As vanilla as can be -- just send it along
								each = ProcessLineElements(each);
								EnsureParaOpen();
								_Output.Write(each);
							}
						}
					}
				}
				EnsureParaClose();
				if (false == currentMultilinePropertyIsHidden)
				{
					_Output.WriteEndLine();
				}
				_CurrentLineIndex++;
			}
    
			_Output.End();

			CurrentState = null;	// Make sure to do this so the last state gets Exit()
		}
		#endregion

		private static string doubleQuote = "\"\"";

		#region ProcessLineElements
		private string ProcessLineElements(string line)
		{
			// Break the line into parts, recognizing escape quotes ("")
			StringBuilder builder = new StringBuilder();
		
			// Build an array of indicies of all the behaviors so we can ignore escapes (double quotes)
			ArrayList behaviors = new ArrayList();
			int off = 0;
			int start, end;
			while (off < line.Length)
			{
				start = line.IndexOf(BehaviorDelimiter, off);
				if (start == -1)
					break;
				end = line.IndexOf(BehaviorDelimiter, start + BehaviorDelimiter.Length);
				if (end == -1)
					break;
				ArrayList pair = new ArrayList();
				pair.Add(start);
				pair.Add(end);
				behaviors.Add(pair);
				off = end + BehaviorDelimiter.Length;
			}

			// OK, now look for paired escape delimiters
			int index = 0;
			while (index < line.Length)
			{
				// find the first open double that's not inside a behavior
				int dq = line.IndexOf(doubleQuote, index);
				while (IsInsideBehavior(dq, behaviors))
					dq = line.IndexOf(doubleQuote, dq + doubleQuote.Length);
				if (dq == -1)
				{
					// none left; process the rest normally
					builder.Append(TranslateBehaviorsAndEverythingElse(line.Substring(index)));
					break;
				}
				
				// Try to find the closing quote
				int close = line.IndexOf(doubleQuote, dq + doubleQuote.Length);
				if (close == -1)
				{
					// We have an unbalanced double, just send the rest along as a literal
					builder.Append(TranslateBehaviorsAndEverythingElse(line.Substring(index)));
					break;
				}

				// Here with an escaped component 
				// First, send along everything up to the component; then send the component untranslated; then continue
				string pre = line.Substring(index, dq - index);
				builder.Append(TranslateBehaviorsAndEverythingElse(pre)); // translated
				builder.Append(line.Substring(dq + doubleQuote.Length, close - dq - doubleQuote.Length));	// untranslated
				index = close + doubleQuote.Length;		
			}
			return builder.ToString();
		}
		#endregion

		#region IsInsideBehavior
		private static bool IsInsideBehavior(int dq, IList behaviors)
		{
			foreach (IList each in behaviors)
			{
				int start = (int)each[0];
				int end = (int)each[1];
				if (start < dq && dq < end)
					return true;
				if (dq < start)
					return false;	// can't be any of the following ones					
			}
			return false;
		}
		#endregion 

		private static string BehaviorDelimiter = "@@";

		#region TranslateBehaviorsAndEverythingElse
		/// <summary>
		/// Break the supplied line into BeL sequences and non-BeL sequences.  Run the BeL interpreter to process
		/// the BeL sequences and do regular wiki processing on the non-BeL sequences
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		private string TranslateBehaviorsAndEverythingElse(string input)
		{
			// Convert foo@bar external ref syntax to standardized InterWiki behavior
			// This must happen here because the actual interpretation of these is as a behavior
			string str = externalWikiRef.Replace(input, new MatchEvaluator(externalWikiRefMatch));		
			
			string escapedTwo = "\\@\\@";
			// If no behaviors, just process normally and return
			if (str.IndexOf(BehaviorDelimiter) == -1)
				return TranslateLineElements(input);
			while (true)
			{
				// Find the next BehaviorDelimiter marker
				int start = str.IndexOf(BehaviorDelimiter);
				if (start == -1)
					break;
				// Find the end
				int end = str.IndexOf(BehaviorDelimiter, start + 2);
				if (end == -1)
					break;
				// OK, we have one
				string expr = str.Substring(start + 2, end - start - 2);
				// Turn any escape sequences into unescaped @@
				expr = expr.Replace(escapedTwo,BehaviorDelimiter);
				TopicContext tc = new TopicContext(TheFederation, ContentBase, new TopicInfo(TheFederation, CurrentTopic));
				BehaviorInterpreter interpreter = new BehaviorInterpreter(expr, TheFederation, TheFederation.WikiTalkVersion, this);
				string replacement = null;
				if (!interpreter.Parse())
				{	// parse failed
					replacement = ErrorMessage(null, interpreter.ErrorString);
				}
				else
				{	// parse succeeded
					if (!interpreter.EvaluateToPresentation(tc, _ExternalWikiMap))
					{	// eval failed
						replacement = ErrorMessage(null, interpreter.ErrorString);
					}
					else
					{	// eval succeeded
						WikiOutput nOut = WikiOutput.ForFormat(Output.Format, Output);
						interpreter.Value.OutputTo(nOut);
						replacement = nOut.ToString();
					}
					foreach (CacheRule r in interpreter.CacheRules)
						AddCacheRule(r);
				}
				str = TranslateLineElements(str.Substring(0, start)) + replacement + TranslateLineElements(str.Substring(end + 2));
			}
			return str;
		}
		#endregion 

		#region TranslateLineElements
		private string TranslateLineElements(string line)
		{
			string answer = line;
			answer = ConvertEmoticons(answer);	// needs to come before textile because of precedence in overlappign formatting rules
			answer = SentencePairedDelimiters(answer);
			answer = TextileFormat(answer);
			answer = LinkHyperLinks(answer);
			answer = ColorsEtcFormat(answer);
			if (answer.IndexOf("<nowiki>") < 0)
			{
				answer = LinkWikiNames(answer);
				answer = ProcessWikiLinks(answer);
			}
			else
			{
				answer = answer.Replace("<nowiki>", "");
			}
			return answer;
		}
		#endregion


		private static Regex externalWikiRef = new Regex(@"(\s)*(?<param>[\w\d\.]+)@(?<behavior>[\w\d]+([\w\d]{1,})+)([\w\d\.])*(\s)*");

		#region ErrorMessage
		public string ErrorMessage(string title, string body)
		{
			// We can't use the fancy ErrorString method -- it didn't exist in v0
			WikiOutput nOut = WikiOutput.ForFormat(Output.Format, Output);
			nOut.WriteErrorMessage(title, body);
			return nOut.ToString();
		}
		#endregion 

		#region externalWikiRefMatch
		private string externalWikiRefMatch(Match match)
		{
			// match.Value may contain some additional leading and/or trailing white space that we don't want to lose.
			string result = match.Value;
			if (!emailAddress.IsMatch(result))
			{
				// Build a string containing the actual match.
				string noSpace = match.Groups["param"].Value + "@" + match.Groups["behavior"].Value;
				// Replace that in the whole match with the external wiki ref macro.
				result = result.Replace(noSpace, "@@InterWiki(\"$behavior\", \"$param\", \"$param\")@@");
				// Replace the parameters with the regex matches.
				result = result.Replace("$behavior", match.Groups["behavior"].Value).Replace("$param", match.Groups["param"].Value);
			}
			return result;
		}
		#endregion 

		#region TheFederation
		private Federation TheFederation
		{
			get
			{
				return ContentBase.Federation;
			}
		}
		#endregion


		#region IncludedTopic
		private string IncludedTopic(TopicName topic, int headingLevelBase)
		{
			// TODO: how do we identify specific versions? [maybe this just works now? since versionids are a formal part of a wikiname???]
			// TODO: how do we show diffs?
			string ns = ContentBase.UnambiguousTopicNamespace(topic);
			ContentBase containingContentBase = TheFederation.ContentBaseForNamespace(ns);
			AbsoluteTopicName abs = new AbsoluteTopicName(topic.Name, ns);
			string content = containingContentBase.Read(abs.LocalName).TrimEnd();
			WikiOutput output = WikiOutput.ForFormat(_Output.Format, Output);
			Formatter.Format(abs, content, output, ContentBase, LinkMaker(), _ExternalWikiMap, headingLevelBase, CacheRuleAccumulator);
			return output.ToString().Trim();
		}
		#endregion


		#region NestedFormat
		/// <summary>
		/// Format the supplied string with the current settings of the Formatter (including to the same Output and cache rule accumulator)
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		private string NestedFormat(string input, WikiOutput parent)
		{
			WikiOutput output = WikiOutput.ForFormat(_Output.Format, parent);
			if (!IsBeyondSafeNestingDepth)
			{
				IList lines = SplitStringIntoStyledLines(input, Output.Style);
				Formatter.Format(CurrentTopic, lines, output, ContentBase, LinkMaker(), _ExternalWikiMap, _HeadingLevelBase, CacheRuleAccumulator);
			}
			else
			{
				/// we're headed for trouble.  The formatting depth is beyond a reasonable depth.  We're likely headed towards
				/// infinite recursion.  
				output.WriteErrorMessage("exceeded maximum nesting depth (" + MaxiumuNestingDepth + ")", "The maximum nesting depth for formatting topics has been exceeded.  This is likely caused by infinite recursion in some WikiTalk.");
			}
			string str = output.ToString();

			// TODO -- Hack alert.  Right now the formatter pretty much always returns its results wrapped in a paragraph.  This is just not
			// what you necessarily want for nested formatting (e.g., if you want just a few words and don't want a para).  So this code strips the 
			// enclosing <p> and </p>; it also takes away any training line break stuff
			string open = "<p>";
			string close = "</p>";
			if (str.StartsWith(open))
			{
				str = str.Remove(0, open.Length);
				int lastOne = str.LastIndexOf(close);
				if (lastOne > 0)
				{
					str = str.Remove(lastOne, close.Length);
				}
			}
			if (str.EndsWith(Environment.NewLine))
				str = str.Substring(0, str.Length - Environment.NewLine.Length);

			// END HACK

			return str;
		}
		#endregion 

		#region ConvertEmoticons
		private string ConvertEmoticons(string input)
		{
			string str = input;
			str = Emote(str, "(Y)", "emoticons/thumbs_up.gif");
			str = Emote(str, "(y)", "emoticons/thumbs_up.gif");
			str = Emote(str, "(N)", "emoticons/thumbs_down.gif");
			str = Emote(str, "(n)", "emoticons/thumbs_down.gif");
			str = Emote(str, "(B)", "emoticons/beer_yum.gif");
			str = Emote(str, "(b)", "emoticons/beer_yum.gif");
			str = Emote(str, "(D)", "emoticons/martini_shaken.gif");
			str = Emote(str, "(d)", "emoticons/martini_shaken.gif");
			str = Emote(str, "(X)", "emoticons/girl_handsacrossamerica.gif");
			str = Emote(str, "(x)", "emoticons/girl_handsacrossamerica.gif");
			str = Emote(str, "(Z)", "emoticons/guy_handsacrossamerica.gif");
			str = Emote(str, "(z)", "emoticons/guy_handsacrossamerica.gif");
			str = Emote(str, "(6)", "emoticons/devil_smile.gif");
			str = Emote(str, ":-[", "emoticons/bat.gif");
			// str = Emote(str, ":[", "emoticons/bat.gif");  // not supported because it conflicts with the topic property syntax
			str = Emote(str, "(})", "emoticons/girl_hug.gif");
			str = Emote(str, "({)", "emoticons/dude_hug.gif");
			str = Emote(str, ":-)", "emoticons/regular_smile.gif");
			str = Emote(str, ":)", "emoticons/regular_smile.gif");
			str = Emote(str, ":-D", "emoticons/teeth_smile.gif");
			// str = Emote(str, ":d", "emoticons/teeth_smile.gif"); // conflicts with urls (e.g., mailto:)
			str = Emote(str, ":-O", "emoticons/omg_smile.gif");
			// str = Emote(str, ":o", "emoticons/omg_smile.gif");  // conflicts with urls (e.g., mailto:)
			str = Emote(str, ":-P", "emoticons/tounge_smile.gif");
			// str = Emote(str, ":p", "emoticons/tounge_smile.gif");  // conflicts with urls (e.g., mailto:)
			str = Emote(str, ";-)", "emoticons/wink_smile.gif");
			str = Emote(str, ";)", "emoticons/wink_smile.gif");
			str = Emote(str, ":-(", "emoticons/sad_smile.gif");
			str = Emote(str, ":(", "emoticons/sad_smile.gif");
			str = Emote(str, ":-S", "emoticons/confused_smile.gif");
			// str = Emote(str, ":s", "emoticons/confused_smile.gif");  // conflicts with urls (e.g., mailto:)
			str = Emote(str, ":-|", "emoticons/whatchutalkingabout_smile.gif");
			str = Emote(str, ":|", "emoticons/whatchutalkingabout_smile.gif");
			str = Emote(str, ":'(", "emoticons/cry_smile.gif");
			str = Emote(str, ":$", "emoticons/embaressed_smile.gif");
			str = Emote(str, ":-$", "emoticons/embaressed_smile.gif");
			str = Emote(str, "(H)", "emoticons/shades_smile.gif");
			str = Emote(str, "(h)", "emoticons/shades_smile.gif");
			str = Emote(str, ":-@", "emoticons/angry_smile.gif");
			str = Emote(str, ":@", "emoticons/angry_smile.gif");
			str = Emote(str, "(A)", "emoticons/angel_smile.gif");
			str = Emote(str, "(a)", "emoticons/angel_smile.gif");
			str = Emote(str, "(L)", "emoticons/heart.gif");
			str = Emote(str, "(l)", "emoticons/heart.gif");
			str = Emote(str, "(U)", "emoticons/broken_heart.gif");
			str = Emote(str, "(u)", "emoticons/broken_heart.gif");
			str = Emote(str, "(K)", "emoticons/kiss.gif");
			str = Emote(str, "(k)", "emoticons/kiss.gif");
			str = Emote(str, "(G)", "emoticons/present.gif");
			str = Emote(str, "(g)", "emoticons/present.gif");
			str = Emote(str, "(F)", "emoticons/rose.gif");
			str = Emote(str, "(f)", "emoticons/rose.gif");
			str = Emote(str, "(W)", "emoticons/wilted_rose.gif");
			str = Emote(str, "(w)", "emoticons/wilted_rose.gif");
			str = Emote(str, "(P)", "emoticons/camera.gif");
			str = Emote(str, "(p)", "emoticons/camera.gif");

			str = Emote(str, "(~)", "emoticons/film.gif");
			str = Emote(str, "(T)", "emoticons/phone.gif");
			str = Emote(str, "(t)", "emoticons/phone.gif");
			str = Emote(str, "(@)", "emoticons/kittykay.gif");
			//			str = Emote(str, "(&)", "emoticons/bowwow.gif");	// disabled because it conflicts with lots of formatting rules
			str = Emote(str, "(C)", "emoticons/coffee.gif");
			str = Emote(str, "(c)", "emoticons/coffee.gif");
			str = Emote(str, "(I)", "emoticons/lightbulb.gif");
			str = Emote(str, "(i)", "emoticons/lightbulb.gif");
			str = Emote(str, "(S)", "emoticons/moon.gif");
			str = Emote(str, "(*)", "emoticons/star.gif");
			str = Emote(str, "(8)", "emoticons/musical_note.gif");
			str = Emote(str, "(E)", "emoticons/envelope.gif");
			str = Emote(str, "(e)", "emoticons/envelope.gif");
			str = Emote(str, "(^)", "emoticons/cake.gif");
			str = Emote(str, "(O)", "emoticons/clock.gif");
			str = Emote(str, "(o)", "emoticons/clock.gif");
			str = Emote(str, "(M)", "emoticons/messenger.gif");
			str = Emote(str, "(m)", "emoticons/messenger.gif");
			return str;
		}
		#endregion

		#region Emote
		private string Emote(string input, string text, string image)
		{
			return input.Replace (text, LinkMaker().LinkToImage(image)) ;			
		}
		#endregion

		#region StripHTMLSpecialCharacters
		private static string StripHTMLSpecialCharacters(string input)
		{
			// replace HTML special characters with character entities
			// this has the side-effect of stripping all markup from text
			string str = input;
			// Special care needs to be taken to be sure & is not stripped out of URLs
			str = Regex.Replace (str, " &", " &amp;") ;

			str = Regex.Replace (str, "<", "&lt;") ;
			str = Regex.Replace (str, ">", "&gt;") ;
			return str;
		}
		#endregion

		#region SentencePairedDelimiters
		private static string SentencePairedDelimiters(string input)
		{
			string str = input;
			// deemphasis
			str = Regex.Replace (str, "`{2}(.*?)`{2}", "<span class=\"Deemphasis\">$1</span>") ;
			// Bold
			str = Regex.Replace (str, "'{3}(.*?)'{3}", "<strong>$1</strong>") ;
			// Italic
			str = Regex.Replace (str, "'{2}(.*?)'{2}", "<em>$1</em>") ;
			return str;
		}
		#endregion
    
		#region TextileFormat
		private string TextileFormat(string input)
		{
			string str = input;
			// _emphasis_
			str = Regex.Replace (str, "(^|\\W)_([^ ].*?)_", "$1<em>$2</em>") ;
			// *strong* 
			str = Regex.Replace (str, "(^|\\W)\\*([^ ].*?)\\*", "$1<strong>$2</strong>") ;
			// ??citation?? 
			str = Regex.Replace (str, "(^|\\W)\\?\\?([^ ].*?)\\?\\?", "$1<cite>$2</cite>") ;
			// -deleted text- 
			// Special care needs to be taken with this expression so that the - in URLs and images does not get replaced
			// and that Wiki Signatures with trailing dates contianing hypens are ignored.
			str = Regex.Replace (str, "(^|\\s+)-([^ <a ]|[^ <img ]|[^ -].*)-", "$1<del>$2</del>");
			// +inserted text+ 
			str = Regex.Replace (str, "(^|\\W)\\+(.*?)\\+", "$1<ins>$2</ins>") ;
			// ^superscript^ 
			str = Regex.Replace (str, "(^|\\W)\\^([^ ].*?)\\^", "$1<sup>$2</sup>") ;
			// ~subscript~ 
			// Special care needs to be taken with this expression so that the ~ in URLs does not get replaced.
			str = Regex.Replace (str, "(^|\\W)~([^ ].*?)~", "$1<sub>$2</sub>") ;
			// @code@ 
			// The regex is a bit special because it needs to start with a zero-width negative lookbehind assertion (because we 
			// need to be sure we don't consume WikiBehaviors @@Whatever@@
			str = Regex.Replace (str, "(^|\\s|\\(|\\[)@([^@]+)@", "$1<code>$2</code>") ;
//			// "link text":url 
//			str = Regex.Replace (str, "\"([^\"(]+)( \\(([^\\)]+)\\))?\":" + urlPattern, ObfuscatableLinkReplacementPattern("$1", "$4"));
//			// "link text":url for mail and news
//			str = Regex.Replace (str, "\"([^\"(]+)( \\(([^\\)]+)\\))?\":" + mailAndNewsPattern, ObfuscatableLinkReplacementPattern("$1", "${uri}"));

			return str;
		}
		#endregion

		#region ColorsEtcFormat
		static string	ColorsEtcFormat( string	input	)
		{
			string str = input;
			// "%([a-zA-Z]+)(	*([a-zA-Z]+))*%"
			bool insideSpan=false;
			int	insideBig=0;
			int	insideSmall=0;

			string elem="([a-zA-Z]*|#[a-fA-F0-9]{6})";
			Regex	style=new	Regex( "%"+elem+"( +"+elem+")*%" );
			Match	match;
			int	lastMatchedPosition=0;
			while	(	(match=style.Match(	str, lastMatchedPosition )).Success	)
			{
				string newValue="";

				// terminate previous	<big>	first	(if	any)
				for	(	;	insideBig	>	0; --insideBig )
				{
					newValue +=	"</big>";
				}

				// terminate previous	<small>	first	(if	any)
				for	(	;	insideSmall	>	0; --insideSmall )
				{
					newValue +=	"</small>";
				}

				// terminate previous	<span> first (if any)
				if ( insideSpan	)
				{
					newValue +=	"</span>";
					insideSpan = false;
				}

				if ( match.Groups[1].Length	>	0	)	// start new block
				{
					for	(	int	i=1; i < match.Groups.Count; i +=	2	)
					{
						string param=match.Groups[i].Value;
						if ( string.Compare( param,	"big", true	)	== 0 )
						{
							++insideBig;
							newValue +=	"<big>";
						}
						else if	(	string.Compare(	param, "small",	true ) ==	0	)
						{
							++insideSmall;
							newValue +=	"<small>";
						}
						else if	(	param	!= string.Empty	)
						{
							if ( insideSpan	)
							{
								newValue
									=	"<span class=\"errormessage\">"
									+		"<span class=\"errormessagetitle\">	Style	Format Error:</span>"
									+		"<span class=\"errormessagebody\">"	+	"	More than	one	color	in \"" + match.Groups[0].Value + "\" " + "</span>"
									+	"</span>"
									;
							}
							else
							{
								insideSpan = true;
								newValue +=	"<span style=\"color:"	+	match.Groups[i].Value.ToLower()	+	"\">";
							}
						}
					}
				}

				// Just	leave	%% alone if	it is	on the line	by itself
				if ( newValue	!= string.Empty	)
				{
					lastMatchedPosition	=	match.Groups[0].Index;
					str	=	str.Remove(	lastMatchedPosition, match.Groups[0].Length	);
					str	=	str.Insert(	lastMatchedPosition, newValue	);
					lastMatchedPosition	+= newValue.Length;
				}
				else
				{
					lastMatchedPosition	+= 2;
				}
			}

			for	(	;	insideBig	>	0; --insideBig )
			{
				str	+= "</big>";
			}

			for	(	;	insideSmall	>	0; --insideSmall )
			{
				str	+= "</small>";
			}

			if ( insideSpan	)
			{
				str	+= "</span>";
			}

			return str;	//	+	debugString;
		}
		#endregion

		static string urlPattern = @"((https?|ftp|gopher|telnet|file|notes|ms-help):((//)|(\\\\))+[\w\d:#@%/;$()~_?\+-=\\\.&]*)";
		static string urlPatternInBrackets = @"((https?|ftp|gopher|telnet|file|notes|ms-help):((//)|(\\\\))+[\w\d:#@%/;$() ~_?\+-=\\\.& ]*)";
		static string mailAndNewsPattern = @"(?<ignore>(^|\s|\W)[\W*])?(?<uri>((mailto|news){1}:[^(//\s,)][\w\d:#@%/;$()~_?\+-=\\\.&]+))";

		private static string bracketedUrlPattern = "\\[(?:"+ urlPatternInBrackets + ")\\]";
		private static string bracketedmailAndNewsPattern = "\\[(?:"+ mailAndNewsPattern + ")\\]";
		private static string unqualifiedUrlPattern = "(?:" + "(?:" + bracketedUrlPattern + ")|(?:" + urlPattern + ")" + ")|(?:" + mailAndNewsPattern + ")|(?:" + bracketedmailAndNewsPattern + ")";
		
		private static string mustHaverelabelPrefix = "(\"(?<relabel>[^\"]+)\"\\:)";
		private static string urlbeforeOrRelabel = "(" + mustHaverelabelPrefix + "|(" + mustHaverelabelPrefix + "|" + beforeWikiName + "))";
		public static string extractUrlLinksString = mustHaverelabelPrefix + "(?<uri>" + unqualifiedUrlPattern + ")" + wikiNameAnchor + afterWikiName;
		public static Regex extractUrlLinks = new Regex(extractUrlLinksString) ;


		#region LinkHyperLinks
		private string LinkHyperLinks (string input)
		{
			string str = string.Empty;
			str = ProcessExternalRelabledLinks(input);
			if (str == string.Empty)
				str = input;

			// image links
			str = Regex.Replace (str, @"([^""']|^)http(://\S*(?i:\.jpg|\.gif|\.png|\.jpeg))",
				"$1<img src=\"HTTPIMAGESOURCE:$2\"/>") ;
			str = Regex.Replace (str, @"([^""'])file(://\S*(?i:\.jpg|\.gif|\.png|\.jpeg|\.doc|\.xls|\.ppt|\.txt))",
				"$1<img src=\"FILEIMAGESOURCE:$2\"/>") ;

			// web links (including those surrounded by parens, brackets and curlies)
			str = Regex.Replace (str, @"[(]" + urlPattern + "[)]", "(" + ObfuscatableLinkReplacementPattern("$1", "$1") + ")") ;
			str = Regex.Replace (str, @"[{]" + urlPattern + "[}]", "{" + ObfuscatableLinkReplacementPattern("$1", "$1") + "}") ;
			str = Regex.Replace (str, @"(^|\s)" + urlPattern, "$1" + ObfuscatableLinkReplacementPattern("$2", "$2"));
			str = Regex.Replace (str, @"[\\[]" + urlPatternInBrackets + "[\\]]", ObfuscatableLinkReplacementPattern("$1", "$1")) ;
			if (str != input)
			{
				int start = str.IndexOf("href=\"");;
				int len = 0;
				while (start > 0)
				{
					len = str.IndexOf("\"", start+6) - start;
					if (len > 0)
					{
						string hrefUri = str.Substring(start, len);
						str = str.Replace(hrefUri, hrefUri.Replace(" ", "%20"));
						start = str.IndexOf("href=\"", start+len+1);
					}
					else
					{
						break;
					}
				}
			
			}

			// mail and news links (including those surrounded by parens, brackets and curlies)
			str = Regex.Replace (str, @"[(]" + mailAndNewsPattern + "[)]", "(" + ObfuscatableLinkReplacementPattern("${uri}", "${uri}") + ")") ;
			str = Regex.Replace (str, @"[{]" + mailAndNewsPattern + "[}]", "{" + ObfuscatableLinkReplacementPattern("${uri}", "${uri}") + "}") ;
			str = Regex.Replace (str, @"[\\[]" + mailAndNewsPattern + "[\\]]", "[" + ObfuscatableLinkReplacementPattern("${uri}", "${uri}") + "]") ;
			MatchCollection matches = Regex.Matches(str, mailAndNewsPattern);
			foreach (Match match in matches)
			{
				Group group = match.Groups["ignore"];
				if (null != group)
				{
					if ((0 == group.Captures.Count) || 
						((group.Captures.Count > 0) && (false == group.Captures[0].Value.EndsWith("\"")) && 
						(false == group.Captures[0].Value.EndsWith(">"))))
					{
						str = Regex.Replace (str, mailAndNewsPattern, ObfuscatableLinkReplacementPattern("${uri}", "${uri}"));
					}
				}
			}
			str = FinalizeImageLinks(str);


			return str;
		}
		#endregion

		#region ProcessExternalRelabledLinks
		private string ProcessExternalRelabledLinks(string input)
		{
			StringBuilder answer = new StringBuilder();
			string str = input;

			while (str.Length > 0)
			{
				Match m = extractUrlLinks.Match(str);
        if (!m.Success)
        {
          break;
        }

				string uri = m.Groups["uri"].ToString();
				string before = m.Groups["before"].ToString();
				string after = m.Groups["after"].ToString();
				string relabel = m.Groups["relabel"].ToString();
				string anchor = m.Groups["anchor"].ToString();

        if (relabel != string.Empty)
        {
          uri = uri.Replace("[", "");
          uri = uri.Replace("]", "");
          uri = uri.Replace(" ", "%20");
          if (anchor != string.Empty)
          {
            uri += "#" + anchor;
          }

          string noFollow = " ";
          if (TheFederation.NoFollowExternalHyperlinks)
          {
            noFollow = " rel=\"nofollow\" ";
          }
					
          str = ReplaceMatch(answer, str, m, before + "<nowiki><a class=\"externalLink\"" + noFollow + "href=\"" + uri + "\">" + relabel + "</a>" + after);
        }
        else
        {
          break;
        }
				
			}
			answer.Append(str);
			return answer.ToString();
		}
		#endregion
	

		#region ObfuscatableLinkReplacementPattern
		private string ObfuscatableLinkReplacementPattern(string replacementText, string replacementURL)
		{
			string noFollow = "";
			if (TheFederation.NoFollowExternalHyperlinks)
				noFollow = " rel=\"nofollow\" ";
			return @"<a class=""externalLink"" " + noFollow + @"href=""" + replacementURL + @""">" + replacementText + @"</a>";
		}
		#endregion

		private static string wikiURIPattern = @"(^|\s+)(?<uri>wiki://(?<authority>" + unbracketedWikiName + @")(?<path>/[a-zA-Z0-9:@%/~_?\-=\\\.]*)?(#(?<fragment>[a-zA-Z0-9:@%/~_?\-=\\\.]*))?)";
		private static Regex wikiURIRegex = new Regex(wikiURIPattern);

		#region ProcessWikiLinks
		private string ProcessWikiLinks(string str)
		{
			MatchCollection matches = wikiURIRegex.Matches(str);
			ArrayList processed = new ArrayList();
			string answer = str;
			foreach (Match m in matches)
			{
				string uriString = m.Groups["uri"].ToString();
				if (processed.Contains(uriString))
				{
					continue;   // skip dup	
				}
				processed.Add(uriString);

				// Get  the pieces
				string authority = m.Groups["authority"].ToString();
				string path = m.Groups["path"].ToString();
				string fragment = m.Groups["fragment"].ToString();
				
				// OK, we have a good URI -- let's decide if it's:
				// wiki://topic
				//		Case 1 - Link to the topic
				//	wiki://topic/rez…
				//		Case 2 - Read the topic, rewrite using the LibraryURL
				// wiki://topic/#property
				//		Case 3 - Inline the property value

				if (path == "" || path == "/")
				{
					// We've got only a topic
					RelativeTopicName topicName = new RelativeTopicName(authority);
					// See if we want the whole topic or just a property (via a fragment)
					if (fragment != "")
					{
						// Case 3 -- Ask for a property
						AbsoluteTopicName abs = null;
						bool ambig = false;
						AddCacheRule(ContentBase.CacheRuleForAllPossibleInstancesOfTopic(topicName));
						try
						{
							abs = ContentBase.UnambiguousTopicNameFor(topicName);
						}
						catch (TopicIsAmbiguousException)
						{
							ambig = true;
						}
						string replacement;
						if (abs != null)
						{
							// Got a unique URI it!
							replacement = TheFederation.GetTopicProperty(abs, fragment);
						}
						else
						{
							if (ambig)
								replacement = "(ambiguous topic name: " + topicName + ")";
							else
								replacement = "(unknown topic name: " + topicName + ")";
						}
						answer = answer.Replace(uriString, replacement);
					}
					else
					{
						// Case 1 - a whole topic
						answer = LinkWikiNames(authority);
					}
				}
				else
				{
					// Case 2 - We're being asked for a resource in a resource library
					AbsoluteTopicName abs = null;
					RelativeTopicName topicName = new RelativeTopicName(authority);
					bool ambig = false;
					AddCacheRule(ContentBase.CacheRuleForAllPossibleInstancesOfTopic(topicName));
					try
					{
						abs = ContentBase.UnambiguousTopicNameFor(topicName);
					}
					catch (TopicIsAmbiguousException)
					{
						ambig = true;
					}
					string replacement;
					if (abs != null)
					{
						// Got a unique URI it!
						string uriForLibrary = TheFederation.GetTopicProperty(abs, "URI");
						string pathWithoutPrefixSlash = path.TrimStart('/');
						string resourceURI = uriForLibrary.Replace("$$$", pathWithoutPrefixSlash);
						replacement = LinkHyperLinks(resourceURI);
					}
					else
					{
						if (ambig)
							replacement = "(ambiguous library topic name: " + topicName + ")";
						else
							replacement = "(unknown library topic name: " + topicName + ")";
					}

					answer = answer.Replace(uriString, replacement);
				}
			}
			return answer;
		}
		#endregion

		#region FinalizeImageLinks
		private string FinalizeImageLinks(string input)
		{
			// finalise image links
			string str = input;
			str = Regex.Replace (str, "HTTPIMAGESOURCE::(//\\S*)", "http:$1") ;
			str = Regex.Replace (str, "FILEIMAGESOURCE::(//\\S*)", "file:$1") ;
			return str;
		}
		#endregion

		#region ReplaceMatch 
		private static string ReplaceMatch (StringBuilder resultQueue, string old, Match m, string rep)
		{
			resultQueue.Append(old.Substring(0, m.Index) + rep);
			return old.Substring(m.Index + m.Length);
		}
		#endregion

		#region NewUniqueIdentifier
		private static int unique = 0;
		private string NewUniqueIdentifier()
		{
			unique++;
      if (unique > 100000)		// wrap ever 100000; nothing magic about this number
      {
        unique = 0;
      }
			return "id" + unique.ToString();
		}
		#endregion

		#region TipForTopic
		private string TipForTopic(AbsoluteTopicName topic)
		{
			string answer = TheFederation.GetTopicProperty(topic, "Summary");
			if (answer == "")
				answer = null;
			return answer;
		}
		#endregion

		#region LinkWikiNames
		private string LinkWikiNames (string input)
		{
			StringBuilder answer = new StringBuilder();

			string str = input;
			ArrayList processed = new ArrayList();
			while (str.Length > 0)
			{
				Match m = extractWikiLinks.Match(str);
				if (!m.Success)
					break;
				string each = m.Groups["topic"].ToString();
				string before = m.Groups["before"].ToString();
				string after = m.Groups["after"].ToString();
				string relabel = m.Groups["relabel"].ToString();
				string anchor = m.Groups["anchor"].ToString();
				
				RelativeTopicName relName = new RelativeTopicName(TopicName.StripEscapes(each));

				// Ignore apparent links to non-existent namespaces.
				if ((null == relName.Namespace) || (null != TheFederation.ContentBaseForNamespace(relName.Namespace)))
				{
					// Build a list of all the possible absoluteNames for this topic
					ArrayList absoluteNames = new ArrayList();
					// Start with the singulars in the various reachable namespaces, then add the plurals
					absoluteNames.AddRange(ContentBase.AllAbsoluteTopicNamesThatExist(relName));
					AddCacheRule(ContentBase.CacheRuleForAllPossibleInstancesOfTopic(relName));
					foreach (TopicName alternate in relName.AlternateForms)
					{
						AddCacheRule(ContentBase.CacheRuleForAllPossibleInstancesOfTopic(alternate));
						absoluteNames.AddRange(ContentBase.AllAbsoluteTopicNamesThatExist(alternate));
					}
					
					// Now see if we got any hits or not
					string rep = beforeOrRelabel + "(" + RegexEscapeTopic(each) + ")" + afterWikiName;
					AbsoluteTopicName appearedAs = new AbsoluteTopicName(each);  // in case it was a plural form, be sure to show it as it appeared
					string displayname = TopicName.StripEscapes((ContentBase.DisplaySpacesInWikiLinks ? appearedAs.FormattedName : appearedAs.Name));
					if (relabel.Length > 0)
					{
						displayname = relabel;
					}

					if (absoluteNames.Count == 0)
					{
						// It doesn't exist, so give the option to create it
						AbsoluteTopicName abs = relName.AsAbsoluteTopicName(ContentBase.Namespace);
						str = ReplaceMatch(answer, str, m, before + "<a title=\"Click here to create this topic\" class=\"create\" href=\"" + LinkMaker().LinkToEditTopic(abs) + "\">" + displayname + "</a>" + after);
					}
					else
					{
						// We got hits, let's add links

						if (absoluteNames.Count == 1)
						{
							// The simple case is that there's only one link to point to, so we output just a normal link
							AbsoluteTopicName abs = (AbsoluteTopicName)absoluteNames[0];
							string tip = TipForTopic(abs);
							string tipid = null;
							string tipHTML = null;
							bool defaultTip = tip == null;
							if (defaultTip)
							{
								tip = "Click to read this topic";
							}
							tipid = NewUniqueIdentifier();
							tipHTML = Formatter.EscapeHTML(tip);
							if (defaultTip)
							{
								tipHTML = "<span class=\"DefaultTopicTipText\">" + tipHTML + "</span>";
							}
							tipHTML += "<div class=\"TopicTipStats\">" + TheFederation.GetTopicModificationTime(abs).ToString() + " - " + TheFederation.GetTopicLastModifiedBy(abs) + "</div>";
							tipHTML = "<div id=" + tipid +" style=\"display: none\">" + tipHTML + "</div>";
							Output.AddToFooter(tipHTML);
							string replacement = "<a ";
							if (tip != null)
							{
								replacement += "onmouseover=\"TopicTipOn(this, '" + tipid + "', event);\" onmouseout=\"TopicTipOff();\" ";
							}
							replacement += "href=\"" + LinkMaker().LinkToTopic(abs);
							if (anchor.Length > 0)
							{
								replacement += "#" + anchor;
								if (0 == relabel.Length)
								{
									displayname += "#" + anchor;
								}
							}
							replacement += "\">" + displayname + "</a>";
							str = ReplaceMatch(answer, str, m, before + replacement + after) ;
						}
						else
						{
							// There's more than one; we need to generate a dynamic menu
							string clickEvent;
							clickEvent = "onclick=\"javascript:LinkMenu(new Array(";
							bool first = true;
							foreach (AbsoluteTopicName eachAbs in absoluteNames)
							{	
								if (!first)
									clickEvent += ", ";
								first = false;
								clickEvent += "new Array('" + eachAbs + "', '" + LinkMaker().LinkToTopic(eachAbs) + "')";
							}
							clickEvent += "), event);\"";

							str = ReplaceMatch(answer, str, m, before + "<a title=\"Different versions of this topic exist.  Click to pick one.\" " + clickEvent + ">" + displayname + "</a>" + after) ;
						}
					}
				}
				else
				{
					str = ReplaceMatch(answer, str, m, before + relName.Fullname + after);
				}
			}

			// Add on whatever's not yet been consumed
			answer.Append(str);
			return answer.ToString();
		}
		#endregion

		#region RegexEscapeTopic
		/// <summary>
		/// Turn a topic (maybe fully-qualified; maybe with brackets) into a legit regex by escaping special regex chars like . and [ and ]
		/// </summary>
		/// <param name="topic"></param>
		/// <returns></returns>
		public static string RegexEscapeTopic(string topic)
		{
			string answer = topic;
			answer = answer.Replace(".", "\\.");
			answer = answer.Replace("[", "\\[");
			answer = answer.Replace("]", "\\]");
			return answer;
		}
		#endregion

		private bool paraOpen = false;
   
		private void EnsureParaOpen()
		{
			if (!paraOpen) { _Output.WriteOpenPara(); paraOpen = true; }
		}
		
		private void EnsureParaClose()
		{
			if (paraOpen) { _Output.WriteClosePara(); paraOpen = false; }
		}
	
		private static Regex externalWikiDef = new Regex("^@(" + Az09 + "+)=(.*)$");
		public static bool StripExternalWikiDef(Hashtable table, string content)
		{
			Match m = externalWikiDef.Match(content);
			if (m.Success)
			{
				table[m.Groups[1].Value] = m.Groups[2].Value;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Escape the given string for < > " and & HTML characters.
		/// </summary>
		/// <param name="input"></param>
		/// <returns>The new string</returns>
		static public string EscapeHTML(string input)
		{
			return escape(input);
		}

		static protected string escape(string input) 
		{
			// replace HTML special characters with character entities
			// this has the side-effect of stripping all markup from text
			string str = input;
			str = Regex.Replace (str, "&", "&amp;") ;
			str = Regex.Replace (str, "\"", "&quot;") ;
			str = Regex.Replace (str, "<", "&lt;") ;
			str = Regex.Replace (str, ">", "&gt;") ;
			return str;
		}

		#region IWikiToPresentation Members

		public string WikiToPresentation(string s)
		{
			return NestedFormat(s, Output);
		}

		#endregion
	}
}
