using Irab;
using System.Data;
using System.Text;

Dictionary<string, AyahIrab> db = new Dictionary<string, AyahIrab>();

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/irab", 
    (HttpRequest request) =>
    {
        var ayahKey = "1:1";
        var startWord = 0;
        var endWord = 0;
        //https://<DOMAIN>/irab?start=2:255:33&end=2:255:37
        var startParam = request.Query["start"].ToString();
        var endParam = request.Query["end"].ToString();

        if(string.IsNullOrEmpty(startParam))
        {
            return new ApiResult() { Status = "error.parameter", Error = "USAGE: domain/irab?start=2:255 OR domain/irab?start=2:255:2 OR domain/irab?start=2:255:2&end=2:255:5" };
        }

        try
        {
            var pieces = startParam.Split(":", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            ayahKey = $"{pieces[0]}:{pieces[1]}";
            if (pieces.Length == 3)
                startWord = Convert.ToInt32(pieces[2]);
            if(!string.IsNullOrEmpty(endParam))
            {
                var epieces = endParam.Split(":", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                endWord = Convert.ToInt32(epieces[2]);
            }
        }
        catch (Exception exc)
        {
            return new ApiResult() { Status = "error.parameter", Error = "USAGE: domain/irab?start=2:255 OR domain/irab?start=2:255:2 OR domain/irab?start=2:255:2&end=2:255:5" };
        }
        if(db.Count==0)
        {
            try
            {
                IrabParser parser = new IrabParser();
                var ayahs = parser.Parse("..\\..\\Data\\");
                db = ayahs.ToDictionary(x => x.Id);
            }
            catch(Exception exc)
            {
                return new ApiResult() { Status = "error.parser", Error = exc.Message };

            }
        }

        if(db.TryGetValue(ayahKey, out AyahIrab result))
        {


            return new ApiResult() { Status = "ok", Verse = result };
        }


        return new ApiResult() { Status = "error.keynotfound" };
       

    }
    
);

app.Run();

public record ApiResult 
{
    public string Status { get; set; }
    public AyahIrab? Verse { get; set; }
    public string? Error { get; set; }
}