using System.Globalization;

namespace PipelineApproval.Models;

public struct TaskLog
{
    private string originalText;

    public TaskLog(string text)
    {
        originalText = text;
        GetTextColor();
        Text = RemoveDateAndCommands();
    }

    public string Text { get; set; }

    public DateTime Time { get; set; }

    public Color TextColor { get; set; }

    private void GetTextColor()
    {
        if (originalText.IndexOf("[error]") >= 0)
        {
            TextColor = Color.FromArgb("#cd4a45");
        }
        else if (originalText.IndexOf("[warning]") >= 0)
        {
            TextColor = Color.FromArgb("#ee972d");
        }
        else if (originalText.IndexOf("[section]") >= 0 
              || originalText.IndexOf("[debug]") >= 0)
        {
            TextColor = Color.FromArgb("#55a362");
        }
        else if (originalText.IndexOf("[command]") >= 0)
        {
            TextColor = Color.FromArgb("#66bdff");
        }
        else
        {
            TextColor = Colors.White;
        }
    }

    private string RemoveDateAndCommands()
    {
        var spaceIndex = originalText.IndexOf(' ');
        Time = DateTime.Parse(originalText[..spaceIndex], CultureInfo.InvariantCulture);

        return originalText[(spaceIndex + 1)..]
            .Replace("##[command]", string.Empty)
            .Replace("##[section]", string.Empty);
    }
}
