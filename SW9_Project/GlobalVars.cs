using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

public static class GlobalVars
{

    /// <summary>
    /// Global variable(s) - sorry guys i needed this info from an unaccesible place. - Elias
    /// </summary>
    static bool TP_lock;
    static bool _targetPractice;
    public static double canvasHeight, canvasWidth;

    //The keys changes every time a image is added to the java part
    //TODO: Get an id printer for the java part
    //WARNING: Duplicate keys causes runtime error, but is not caught on compile time.
    public static readonly Dictionary<int,BitmapImage > imgDict =
        new Dictionary<int, BitmapImage>
        {
            { 0,          new BitmapImage(new Uri("resources/DocumentImage.png", UriKind.RelativeOrAbsolute)) }, //document / default
            { 2130837568, new BitmapImage(new Uri("resources/cat.jpg", UriKind.RelativeOrAbsolute)) }, //cat
            { 2130837576, new BitmapImage(new Uri("resources/flower.jpg", UriKind.RelativeOrAbsolute)) }, //flower
            { 2130837582, new BitmapImage(new Uri("resources/sky.jpg", UriKind.RelativeOrAbsolute)) }, //sky
            { 2130837586, new BitmapImage(new Uri("resources/china.jpg", UriKind.RelativeOrAbsolute)) }, //china
            { 2130837588, new BitmapImage(new Uri("resources/tiger.jpg", UriKind.RelativeOrAbsolute)) },  //tiger
            //additional pictures (06 april)
            { 2130837590, new BitmapImage(new Uri("resources/ad.png", UriKind.RelativeOrAbsolute)) },  //ad -
            { 2130837566, new BitmapImage(new Uri("resources/batman.jpg", UriKind.RelativeOrAbsolute)) },  //batman
            { 2130837570, new BitmapImage(new Uri("resources/church.jpg", UriKind.RelativeOrAbsolute)) },  //church
            { 2130837578, new BitmapImage(new Uri("resources/heart.jpg", UriKind.RelativeOrAbsolute)) },  //heart
            { 2130837580, new BitmapImage(new Uri("resources/marilyn.jpg", UriKind.RelativeOrAbsolute)) }  //marilyn



        };

    //static Dictionary<string, string> _dict = new Dictionary<string, string>
    //{
    //{"entry", "entries"},
    //{"image", "images"},
    //{"view", "views"},
    //{"file", "files"},
    //{"result", "results"},
    //{"word", "words"},
    //{"definition", "definitions"},
    //{"item", "items"},
    //{"megabyte", "megabytes"},
    //{"game", "games"}
    //};

    public static bool isTargetPractice
    {
        get
        {
            return _targetPractice;
        }
        set
        {
            if (!TP_lock) //lazy singleton
            {
                _targetPractice = value;
                TP_lock = true;
            }
        }
    }

    public static readonly List<string> docStrings = new List<string>(new string[] { "Job advertisement", "School regulations", "Party invitation", "Fitness brochure", "Study survey", "Cantina menu", "Health consulting", "Student offers in Aalborg", "Thesis final_v2", "Keys missing, help!”, ” Chocolate cake recipe", "Used books sale", "Free beer on Friday", "Student of the year", "Professor of the year", "Annual F-club meeting minutes"  });

}