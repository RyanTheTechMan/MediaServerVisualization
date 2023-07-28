using System;

public class StringManipulation {
    public static string ReplaceLastXPercentOfString(string original, double percentage, char replacement) {
        if (percentage < 0 || percentage > 1)
            throw new ArgumentException("Percentage must be between 0 and 1.");

        int replaceCount = (int)(original.Length * percentage);
        int startReplaceIndex = original.Length - replaceCount;

        char[] arr = original.ToCharArray();
        for (int i = startReplaceIndex; i < original.Length; i++) {
            arr[i] = replacement;
        }

        return new string(arr);
    }
}