using System.Text;

namespace Snobol4.Common;

public partial class Builder
{
    public void DisplaySignOnBanner()
    {
        if (!BuildOptions.SuppressSignOnMessage)
            DisplayBanner(Console.Out);
    }

    // SPITBOL's listing (-LIST, -l): the first listed line brings the head (three blank lines, the identity and
    // clock lines unless -h, three more blank lines) and every page opens with "page N" after the page width
    // less eight blanks (112 at the default 120, -t sets it) and a blank line. A page holds -g less five
    // listed lines (55 at the default 60), the first four fewer under the head, and the next page starts with
    // three blank lines. Under -p or -z, the long format, a form feed takes the place of the three blank lines
    // (before the head and between pages) and a page holds -g less two lines. As sbl lays it out.
    private int _listingPage;
    private int _listingLinesOnPage;

    // What -TITLE and -STITL set for the head of the pages that follow: the title (empty at first), the subtitle (none at first; -TITLE
    // makes it an empty one, so that its line is blank), and whether the next line listed begins a new page under them
    private string _listingTitle = "";
    private string? _listingSubtitle;
    private bool _listingTitlePending;

    private bool LongListing => BuildOptions.PrinterListing || BuildOptions.FormFeedListing;

    // The identity line of the listing head: this compiler's own (whether to print SPITBOL's is Lon's question,
    // as for HOST())
    private const string ListingIdentity = "snobol4dotnet version 0.1";

    // The listing is opened by what is listed first: its head and its first page
    private bool OpenListing()
    {
        if (_listingPage != 0)
            return false;
        var writer = ListingWriter;
        WriteListingHead(writer);
        StartListingPage(writer, BuildOptions.SuppressListingHeader ? 0 : 4);
        return true;
    }

    private void NewListingPage()
    {
        var writer = ListingWriter;
        writer.Write(LongListing ? "\f" : "\n\n\n");
        StartListingPage(writer, 0);
    }

    // A source line, control statement or comment for the listing, double-spaced under -DOUBLE; it is the first line of a new page
    // when -TITLE or -STITL came since the last one listed
    internal void WriteListedLine(string line, bool doubleSpaced)
    {
        if (_listingTitlePending)
        {
            _listingTitlePending = false;
            if (_listingPage > 0)
                NewListingPage();
        }

        WriteListingLine(line);
        if (doubleSpaced)
        {
            ListingWriter.Write("\n");
            _listingLinesOnPage++;
        }
    }

    // -TITLE and -STITL: the head of the pages that follow
    internal void SetListingTitle(string title, string? subtitle)
    {
        _listingTitle = title;
        _listingSubtitle = subtitle;
        _listingTitlePending = true;
    }

    internal void SetListingSubtitle(string subtitle)
    {
        _listingSubtitle = subtitle;
        _listingTitlePending = true;
    }

    // -EJECT: a new page (the first page, when nothing is listed yet, is the one the next line opens)
    internal void ListingEject()
    {
        if (!OpenListing())
            NewListingPage();
    }

    // -SPACE n: n blank lines, or a new page when they do not fit (and nothing before the page is open)
    internal void ListingSpace(int lines)
    {
        if (OpenListing())
            return;
        if (_listingLinesOnPage + lines < BuildOptions.LinesPerPage - (LongListing ? 2 : 5))
        {
            ListingWriter.Write(new string('\n', lines));
            _listingLinesOnPage += lines;
        }
        else
            NewListingPage();
    }

    internal void WriteListingLine(string line)
    {
        var writer = ListingWriter;
        if (OpenListing())
        {
        }
        else if (_listingPage > 0 && _listingLinesOnPage >= BuildOptions.LinesPerPage - (LongListing ? 2 : 5))
            NewListingPage();

        // a line longer than the width of the listing is cut into lines of that width, mid-word, as sbl prints it; it takes one
        // line of the page
        WriteWrapped(writer, line);
        _listingLinesOnPage++;
    }

    // The pieces of a line of at most `width` bytes (in UTF-8, as sbl counts them) that it is cut into; one when it fits
    internal static List<string> WrapLine(string line, int width)
    {
        var pieces = new List<string>();
        if (width <= 0 || Encoding.UTF8.GetByteCount(line) <= width)
        {
            pieces.Add(line);
            return pieces;
        }

        var start = 0;
        while (true)
        {
            var bytes = 0;
            var end = start;
            while (end < line.Length)
            {
                var step = char.IsHighSurrogate(line[end]) && end + 1 < line.Length ? 2 : 1;
                var size = Encoding.UTF8.GetByteCount(line.AsSpan(end, step));
                if (bytes + size > width)
                    break;
                bytes += size;
                end += step;
            }

            if (end >= line.Length)
            {
                pieces.Add(line[start..]);
                return pieces;
            }

            pieces.Add(line[start..end]);
            start = end;
        }
    }

    // The head of a listing: three blank lines (a form feed under -p and -z), the identity and clock lines unless -h, and three
    // blank lines
    private void WriteListingHead(TextWriter writer)
    {
        writer.Write(LongListing ? "\f" : "\n\n\n");
        if (BuildOptions.SuppressListingHeader)
            return;
        var now = DateTime.Now;
        var platform = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture switch
        {
            System.Runtime.InteropServices.Architecture.X64 => "x86-64",
            var other => other.ToString().ToLowerInvariant()
        };
        writer.Write($"{ListingIdentity}\n{platform}  {now:ddd MMM} {now.Day,2} {now:HH:mm:ss yyyy}\n\n\n\n");
    }

    // The head of a page: the title with "page N" over it at the page width less eight (a title wider than the page is cut into lines
    // first, and the page text goes over the last of them; it takes what is under it from a title that reaches it), the subtitle if
    // there is one, and a blank line
    private void StartListingPage(TextWriter writer, int used)
    {
        _listingPage++;
        var offset = BuildOptions.ListingPageFlushLeft ? 0 : Math.Max(0, BuildOptions.PageWidth - 8);
        var pageText = $"page {_listingPage}";
        var title = _listingTitle;
        for (; title.Length > BuildOptions.PageWidth && BuildOptions.PageWidth > 0; title = title[BuildOptions.PageWidth..])
            writer.Write(title[..BuildOptions.PageWidth] + "\n");
        if (title.Length < offset)
            title = title.PadRight(offset);
        var line = title[..offset] + pageText + (title.Length > offset + pageText.Length ? title[(offset + pageText.Length)..] : "");
        WriteWrapped(writer, line);
        _listingLinesOnPage = used;
        if (_listingSubtitle is not null)
        {
            WriteWrapped(writer, _listingSubtitle);
            _listingLinesOnPage++;
        }

        writer.Write("\n");
    }

    private void WriteWrapped(TextWriter writer, string line)
    {
        foreach (var piece in WrapLine(line, BuildOptions.PageWidth))
            writer.Write(piece + "\n");
    }

    private static void DisplayBanner(TextWriter writer)
    {
        var now = DateTime.Now;
        var banner = $"""
                      Snobol4.NET - Multi-platform - v 0.1
                      Copyright © 2024-{now.Year} Jeffrey A. Cooper. Covered by MIT License https://opensource.org/license/mit
                      {now.DayOfWeek} {now.Day} {now:MMMM} {now.Year} {now:HH:mm:ss}
                                                                    

                      
                      """;
        writer.WriteLine(banner);
    }
}