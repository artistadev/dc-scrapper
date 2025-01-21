// See https://aka.ms/new-console-template for more information
using dc_scrapper;
using HtmlAgilityPack;
using System.Text.Json;
using System.Text.RegularExpressions;

var baseUrl = "https://www.dc.com/characters?page={0}";
var characterUrls = new List<string>();
for (int i = 1; i <= 13; i++)
{
    var url = string.Format(baseUrl, i);

    HtmlWeb web = new();

    var htmlDoc = web.Load(url);


    var node = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='sc-kp9rqo-0 hatPuZ resultsContainer results-grid']");

    var nodes = node.SelectNodes(".//a[@class='card-button usePointer']");

    foreach (var item in nodes)
    {
        var characterUrl = item.GetAttributeValue("href", "");
        if (url != null)
        {
            characterUrls.Add("https://www.dc.com" + characterUrl);
        }
    }

}

//if (characterUrls.Count > 0)
//    File.WriteAllLines("url.txt", characterUrls);


List<Character> characterList = [];
var urls = File.ReadAllLines("url.txt");
foreach (var url in characterUrls)
{
    try
    {
        HtmlWeb web = new();
        HtmlDocument doc = web.Load(url);

        Character character = new();
        var docNode = doc.DocumentNode;
        var textNode = docNode.SelectSingleNode("//div[@id='page99-band6808-Text6809']");

        // Get Character Name
        character.Name = textNode?.SelectSingleNode(".//h1")?.InnerText?.Trim();

        // Get Character Details
        var detailLines = textNode?.SelectNodes(".//p");
        if (detailLines != null)
        {
            foreach (var line in detailLines)
            {
                var text = line.InnerText;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    character.Description.Add(text);
                }
            }
        }

        // Get Character Thumbnail
        var thumbnailSection = docNode.SelectSingleNode("//section[@id='page99-band6808']");
        if (thumbnailSection != null)
        {
            var imageNode = thumbnailSection.SelectSingleNode(".//img");
            if (imageNode != null)
            {
                character.Thumbnail = imageNode?.GetAttributeValue("src", "");
            }
        }

        // Get Character Facts
        var factSection = docNode.SelectSingleNode("//section[@id='page99-band6744']");
        var labels = factSection?.SelectNodes(".//div[@aria-label='list-label']");
        var labelValues = factSection?.SelectNodes(".//div[@aria-label='list-values']");
        if (labels != null && labelValues != null)
        {
            for (int i = 0; i < labels.Count; i++)
            {
                var labelText = labels[i]?.SelectSingleNode(".//p")?.InnerText;
                var LabelValueText = labelValues[i].SelectSingleNode(".//p")?.InnerText;

                if (labelText != null && LabelValueText != null)
                {
                    character.Facts.Add(new KeyValuePair<string, string>(labelText, LabelValueText));
                }
            }
        }

        // Get Related Characters
        var relatedSection = docNode.SelectSingleNode("//div[@id='page99-band6824-ContentGrid6827']");
        if (relatedSection != null)
        {
            var charColumns = relatedSection.SelectNodes(".//div[@class='col col-custom']");
            if (charColumns != null && charColumns.Count > 0)
            {
                foreach (var charColumn in charColumns)
                {
                    var imgUrl = charColumn.SelectSingleNode(".//img")?.GetAttributeValue("src", "");
                    var uri = charColumn.SelectSingleNode(".//a")?.GetAttributeValue("href", "");
                    var name = charColumn.SelectSingleNode(".//div[@class='card-title']")?.InnerText;

                    if (!string.IsNullOrWhiteSpace(imgUrl) && !string.IsNullOrWhiteSpace(uri) && !string.IsNullOrWhiteSpace(name))
                    {
                        character.RelatedCharacters.Add(new Related { Name = name, Thumbnail = imgUrl, Uri = uri });
                    }
                }
            }
        }

        // Get Banners Related to Character
        var styleNode = doc.DocumentNode.SelectSingleNode("//style");
        if (styleNode != null)
        {
            string styleContent = styleNode.InnerText;

            var className = "variant-e";
            // Extract the CSS for the specified class
            var data = ExtractCssForClass(styleContent, className);
            if (data != null)
                foreach (var imgUrl in data)
                    character.BannerImages.Add(imgUrl);

        }
        
        string dataJson = System.Text.Json.JsonSerializer.Serialize(character, options: new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(dataJson);
        Console.WriteLine("Scrapping done for URL: " + url);
        Console.WriteLine("-----------------------------------------------------------------------------");
       
        characterList.Add(character);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error Scrapping for URL: " + url, ex);
    }
}

string jsonString = System.Text.Json.JsonSerializer.Serialize(characterList, options: new JsonSerializerOptions { WriteIndented = true });

//File.WriteAllText("data.json", jsonString);

Console.WriteLine("Scrapping Finished.........................................");

// Helper method to extract CSS for a specific class
static HashSet<string> ExtractCssForClass(string css, string className)
{
    string classSelector = $".{className}";
    int index = 0;
    var result = new HashSet<string>();

    while ((index = css.IndexOf(classSelector, index)) != -1)
    {
        // Find the opening brace of the CSS block
        int openBraceIndex = css.IndexOf('{', index);
        if (openBraceIndex == -1) break;

        // Find the closing brace of the CSS block
        int closeBraceIndex = css.IndexOf('}', openBraceIndex);
        if (closeBraceIndex == -1) break;

        // Extract the CSS block
        string cssBlock = css.Substring(openBraceIndex + 1, closeBraceIndex - openBraceIndex - 1).Trim();
        var data = ExtractGalleryImageUrl(cssBlock);
        if (!string.IsNullOrWhiteSpace(data))
            result.Add(data);

        // Move the index past the current block
        index = closeBraceIndex + 1;
    }
    return result;
}

static string ExtractGalleryImageUrl(string cssText)
{
    // Regular expression to extract the URL from background-image
    string pattern = @"background-image:\s*url\(([""']?)(?<url>.*?)\1\)";
    Match match = Regex.Match(cssText, pattern);

    if (match.Success)
    {
        string imageUrl = match.Groups["url"].Value;
        Console.WriteLine("Extracted URL: " + imageUrl);
        return imageUrl;
    }
    
    return String.Empty;
}


