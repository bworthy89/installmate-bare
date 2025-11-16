using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using System.Text.RegularExpressions;

namespace InstallVibe.Converters;

/// <summary>
/// Converts markdown text to XAML content for display in a RichTextBlock.
/// Supports basic markdown: headers, bold, italic, lists, code blocks.
/// </summary>
public static partial class MarkdownToXamlConverter
{
    // Attached property for binding markdown content in XAML
    public static readonly DependencyProperty MarkdownContentProperty =
        DependencyProperty.RegisterAttached(
            "MarkdownContent",
            typeof(string),
            typeof(MarkdownToXamlConverter),
            new PropertyMetadata(null, OnMarkdownContentChanged));

    public static string? GetMarkdownContent(DependencyObject obj)
    {
        return (string?)obj.GetValue(MarkdownContentProperty);
    }

    public static void SetMarkdownContent(DependencyObject obj, string? value)
    {
        obj.SetValue(MarkdownContentProperty, value);
    }

    private static void OnMarkdownContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RichTextBlock richTextBlock)
        {
            ConvertMarkdownToRichTextBlock(e.NewValue as string, richTextBlock);
        }
    }

    public static void ConvertMarkdownToRichTextBlock(string? markdown, RichTextBlock richTextBlock)
    {
        if (string.IsNullOrWhiteSpace(markdown) || richTextBlock == null)
        {
            return;
        }

        richTextBlock.Blocks.Clear();

        // Split by lines and process each one
        var lines = markdown.Split('\n');
        Paragraph? currentParagraph = null;
        var isInCodeBlock = false;
        var codeBlockLines = new List<string>();

        foreach (var line in lines)
        {
            var trimmedLine = line.TrimEnd();

            // Handle code blocks
            if (trimmedLine.StartsWith("```"))
            {
                if (isInCodeBlock)
                {
                    // End code block
                    AddCodeBlock(richTextBlock, string.Join("\n", codeBlockLines));
                    codeBlockLines.Clear();
                    isInCodeBlock = false;
                }
                else
                {
                    // Start code block
                    isInCodeBlock = true;
                }
                continue;
            }

            if (isInCodeBlock)
            {
                codeBlockLines.Add(trimmedLine);
                continue;
            }

            // Empty lines
            if (string.IsNullOrWhiteSpace(trimmedLine))
            {
                if (currentParagraph != null)
                {
                    richTextBlock.Blocks.Add(currentParagraph);
                    currentParagraph = null;
                }
                continue;
            }

            // Headers
            if (trimmedLine.StartsWith("### "))
            {
                if (currentParagraph != null)
                {
                    richTextBlock.Blocks.Add(currentParagraph);
                }
                currentParagraph = CreateHeaderParagraph(trimmedLine[4..], 18);
                richTextBlock.Blocks.Add(currentParagraph);
                currentParagraph = null;
                continue;
            }
            if (trimmedLine.StartsWith("## "))
            {
                if (currentParagraph != null)
                {
                    richTextBlock.Blocks.Add(currentParagraph);
                }
                currentParagraph = CreateHeaderParagraph(trimmedLine[3..], 22);
                richTextBlock.Blocks.Add(currentParagraph);
                currentParagraph = null;
                continue;
            }
            if (trimmedLine.StartsWith("# "))
            {
                if (currentParagraph != null)
                {
                    richTextBlock.Blocks.Add(currentParagraph);
                }
                currentParagraph = CreateHeaderParagraph(trimmedLine[2..], 28);
                richTextBlock.Blocks.Add(currentParagraph);
                currentParagraph = null;
                continue;
            }

            // Bullet lists
            if (trimmedLine.StartsWith("- ") || trimmedLine.StartsWith("* "))
            {
                if (currentParagraph != null)
                {
                    richTextBlock.Blocks.Add(currentParagraph);
                }
                currentParagraph = CreateBulletParagraph(trimmedLine[2..]);
                richTextBlock.Blocks.Add(currentParagraph);
                currentParagraph = null;
                continue;
            }

            // Numbered lists
            var numberedListMatch = NumberedListRegex().Match(trimmedLine);
            if (numberedListMatch.Success)
            {
                if (currentParagraph != null)
                {
                    richTextBlock.Blocks.Add(currentParagraph);
                }
                currentParagraph = CreateNumberedParagraph(numberedListMatch.Groups[1].Value, trimmedLine[(numberedListMatch.Length)..]);
                richTextBlock.Blocks.Add(currentParagraph);
                currentParagraph = null;
                continue;
            }

            // Regular paragraph
            if (currentParagraph == null)
            {
                currentParagraph = new Paragraph();
            }
            else
            {
                currentParagraph.Inlines.Add(new LineBreak());
            }

            AddFormattedText(currentParagraph, trimmedLine);
        }

        // Add final paragraph
        if (currentParagraph != null)
        {
            richTextBlock.Blocks.Add(currentParagraph);
        }

        // Add final code block if still open
        if (isInCodeBlock && codeBlockLines.Count > 0)
        {
            AddCodeBlock(richTextBlock, string.Join("\n", codeBlockLines));
        }
    }

    private static Paragraph CreateHeaderParagraph(string text, double fontSize)
    {
        var paragraph = new Paragraph
        {
            FontSize = fontSize,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 12, 0, 8)
        };
        AddFormattedText(paragraph, text);
        return paragraph;
    }

    private static Paragraph CreateBulletParagraph(string text)
    {
        var paragraph = new Paragraph
        {
            Margin = new Thickness(0, 4, 0, 4)
        };
        paragraph.Inlines.Add(new Run { Text = "• ", FontWeight = FontWeights.Bold });
        AddFormattedText(paragraph, text);
        return paragraph;
    }

    private static Paragraph CreateNumberedParagraph(string number, string text)
    {
        var paragraph = new Paragraph
        {
            Margin = new Thickness(0, 4, 0, 4)
        };
        paragraph.Inlines.Add(new Run { Text = $"{number}. ", FontWeight = FontWeights.Bold });
        AddFormattedText(paragraph, text);
        return paragraph;
    }

    private static void AddCodeBlock(RichTextBlock richTextBlock, string code)
    {
        var paragraph = new Paragraph
        {
            FontFamily = new FontFamily("Consolas"),
            Foreground = Application.Current.Resources["TextFillColorSecondaryBrush"] as Brush,
            Margin = new Thickness(0, 8, 0, 8)
        };

        var border = new Border
        {
            Background = Application.Current.Resources["SubtleFillColorSecondaryBrush"] as Brush,
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(12),
            Margin = new Thickness(0, 8, 0, 8)
        };

        paragraph.Inlines.Add(new Run { Text = code });
        richTextBlock.Blocks.Add(paragraph);
    }

    private static void AddFormattedText(Paragraph paragraph, string text)
    {
        // Handle inline formatting: **bold**, *italic*, `code`
        var parts = SplitByFormatting(text);

        foreach (var part in parts)
        {
            if (part.StartsWith("**") && part.EndsWith("**") && part.Length > 4)
            {
                paragraph.Inlines.Add(new Run
                {
                    Text = part[2..^2],
                    FontWeight = FontWeights.Bold
                });
            }
            else if (part.StartsWith("*") && part.EndsWith("*") && part.Length > 2)
            {
                paragraph.Inlines.Add(new Run
                {
                    Text = part[1..^1],
                    FontStyle = Windows.UI.Text.FontStyle.Italic
                });
            }
            else if (part.StartsWith("`") && part.EndsWith("`") && part.Length > 2)
            {
                paragraph.Inlines.Add(new Run
                {
                    Text = part[1..^1],
                    FontFamily = new FontFamily("Consolas"),
                    Foreground = Application.Current.Resources["AccentTextFillColorPrimaryBrush"] as Brush
                });
            }
            else
            {
                paragraph.Inlines.Add(new Run { Text = part });
            }
        }
    }

    private static List<string> SplitByFormatting(string text)
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();

        for (int i = 0; i < text.Length; i++)
        {
            if (i < text.Length - 1)
            {
                // Check for **
                if (text[i] == '*' && text[i + 1] == '*')
                {
                    if (current.Length > 0)
                    {
                        result.Add(current.ToString());
                        current.Clear();
                    }

                    // Find closing **
                    int closeIndex = text.IndexOf("**", i + 2);
                    if (closeIndex > 0)
                    {
                        result.Add(text.Substring(i, closeIndex - i + 2));
                        i = closeIndex + 1;
                        continue;
                    }
                }
            }

            // Check for * (italic)
            if (text[i] == '*')
            {
                if (current.Length > 0)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }

                int closeIndex = text.IndexOf('*', i + 1);
                if (closeIndex > 0)
                {
                    result.Add(text.Substring(i, closeIndex - i + 1));
                    i = closeIndex;
                    continue;
                }
            }

            // Check for ` (code)
            if (text[i] == '`')
            {
                if (current.Length > 0)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }

                int closeIndex = text.IndexOf('`', i + 1);
                if (closeIndex > 0)
                {
                    result.Add(text.Substring(i, closeIndex - i + 1));
                    i = closeIndex;
                    continue;
                }
            }

            current.Append(text[i]);
        }

        if (current.Length > 0)
        {
            result.Add(current.ToString());
        }

        return result;
    }

    [GeneratedRegex(@"^(\d+)\.?\s")]
    private static partial Regex NumberedListRegex();
}
