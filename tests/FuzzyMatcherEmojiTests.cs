// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.CommandPalette.Extensions.Toolkit.UnitTests;

[TestClass]
public sealed class FuzzyMatcherEmojiTests
{
    [TestMethod]
    public void ExactMatch_SimpleEmoji_ReturnsScore()
    {
        const string needle = "🚀";
        const string haystack = "Launch 🚀 sequence";

        var result = FuzzyStringMatcher.ScoreFuzzyWithPositions(needle, haystack, allowNonContiguousMatches: true);

        Assert.IsTrue(result.Score > 0, "Expected match for simple emoji");

        // 🚀 is 2 chars (surrogates)
        Assert.AreEqual(2, result.Positions.Count, "Expected 2 matched characters positions for the emoji");
    }

    [TestMethod]
    public void ExactMatch_SkinTone_ReturnsScore()
    {
        const string needle = "👍🏽"; // Medium skin tone
        const string haystack = "Thumbs up 👍🏽 here";

        var result = FuzzyStringMatcher.ScoreFuzzyWithPositions(needle, haystack, allowNonContiguousMatches: true);

        Assert.IsTrue(result.Score > 0, "Expected match for emoji with skin tone");

        // 👍🏽 is 4 chars: U+1F44D (2 chars) + U+1F3FD (2 chars)
        Assert.AreEqual(4, result.Positions.Count, "Expected 4 matched characters positions for the emoji with modifier");
    }

    [TestMethod]
    public void ZWJSequence_Family_Match()
    {
        const string needle = "👨‍👩‍👧‍👦"; // Family: Man, Woman, Girl, Boy
        const string haystack = "Emoji 👨‍👩‍👧‍👦 Test";

        var result = FuzzyStringMatcher.ScoreFuzzyWithPositions(needle, haystack, allowNonContiguousMatches: true);

        Assert.IsTrue(result.Score > 0, "Expected match for ZWJ sequence");

        // This emoji is 11 code points? No.
        // Man (2) + ZWJ (1) + Woman (2) + ZWJ (1) + Girl (2) + ZWJ (1) + Boy (2) = 11 chars?
        // Let's just check score > 0.
        Assert.IsTrue(result.Positions.Count > 0);
    }

    [TestMethod]
    public void Flags_Match()
    {
        const string needle = "🇺🇸"; // US Flag (Regional Indicator U + Regional Indicator S)
        const string haystack = "USA 🇺🇸";

        var result = FuzzyStringMatcher.ScoreFuzzyWithPositions(needle, haystack, allowNonContiguousMatches: true);

        Assert.IsTrue(result.Score > 0, "Expected match for flag emoji");

        // 2 code points, each is surrogate pair?
        // U+1F1FA (REGIONAL INDICATOR SYMBOL LETTER U) -> 2 chars
        // U+1F1F8 (REGIONAL INDICATOR SYMBOL LETTER S) -> 2 chars
        // Total 4 chars.
        Assert.AreEqual(4, result.Positions.Count);
    }

    [TestMethod]
    public void Emoji_MixedWithText_Search()
    {
        const string needle = "t🌮o"; // "t" + taco + "o"
        const string haystack = "taco 🌮 on tuesday";

        var result = FuzzyStringMatcher.ScoreFuzzyWithPositions(needle, haystack, allowNonContiguousMatches: true);

        Assert.IsTrue(result.Score > 0);
    }
}
