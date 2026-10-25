using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace SreToolbox.Views;

public sealed partial class CronPage : Page
{
    private static readonly string[] MonthNames =
        { "", "January", "February", "March", "April", "May", "June",
          "July", "August", "September", "October", "November", "December" };

    private static readonly string[] DayNames =
        { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

    public CronPage()
    {
        this.InitializeComponent();
    }

    private void ExplainButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Text = string.Empty;
        OutputText.Text = string.Empty;
        var fields = CronBox.Text.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (fields.Length != 5)
        {
            ErrorText.Text = "Enter exactly 5 fields: minute hour day-of-month month day-of-week.";
            return;
        }
        try
        {
            var lines = new[]
            {
                $"Minute:       {Phrase(fields[0], "minute", "minutes")}",
                $"Hour:         {Phrase(fields[1], "hour", "hours")}",
                $"Day of month: {Phrase(fields[2], "day of month", "days of month")}",
                $"Month:        {Phrase(fields[3], "month", "months", i => MonthNames[i])}",
                $"Day of week:  {Phrase(fields[4], "day of week", "days of week", i => DayNames[i])}",
            };
            OutputText.Text = string.Join(Environment.NewLine, lines);
        }
        catch (FormatException)
        {
            ErrorText.Text = "Could not parse that expression. Use numbers, *, lists (1,2,3), ranges (1-5), and steps (*/15).";
        }
        catch (OverflowException)
        {
            ErrorText.Text = "A field value is too large to parse.";
        }
    }

    private static string Phrase(string field, string singular, string plural, Func<int, string>? name = null)
    {
        string V(int i) => name is not null ? name(i) : i.ToString();

        if (field == "*") return $"every {singular}";

        if (field.StartsWith("*/"))
            return $"every {field.Substring(2)} {plural}";

        if (field.Contains('/'))
        {
            var parts = field.Split('/');
            var step = parts[1];
            var basePart = parts[0];
            if (basePart.Contains('-'))
            {
                var r = basePart.Split('-');
                return $"every {step} {plural}, from {V(int.Parse(r[0]))} to {V(int.Parse(r[1]))}";
            }
            return $"every {step} {plural}, starting at {V(int.Parse(basePart))}";
        }

        if (field.Contains(','))
        {
            var vals = field.Split(',').Select(p =>
                p.Contains('-')
                    ? $"{V(int.Parse(p.Split('-')[0]))}-{V(int.Parse(p.Split('-')[1]))}"
                    : V(int.Parse(p)));
            return $"{plural} {string.Join(", ", vals)}";
        }

        if (field.Contains('-'))
        {
            var r = field.Split('-');
            return $"{plural} from {V(int.Parse(r[0]))} to {V(int.Parse(r[1]))}";
        }

        return $"{singular} {V(int.Parse(field))}";
    }
}
