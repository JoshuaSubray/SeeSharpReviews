namespace SeeSharpReviews.Models;

public class Movie
{
    public int Id { get; set; } // TMDb movie ID
    public string Title { get; set; } = string.Empty; // Movie title
    public string Tagline { get; set; } = string.Empty; // Short tagline
    public string Overview { get; set; } = string.Empty; // Plot summary
    public string PosterPath { get; set; } = string.Empty; // Partial poster image path
    public string ReleaseDate { get; set; } = string.Empty; // Release date
    public int Runtime { get; set; } // Length in minutes
    public double VoteAverage { get; set; } // TMDb rating out of 10
    public string Status { get; set; } = string.Empty; // Production status of the movie
    public List<string> Genres { get; set; } = new List<string>(); // List of genre names
}
