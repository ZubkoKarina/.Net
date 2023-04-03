using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace MusicApp
{
    public class Artist
    {
        public int ArtistId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int CountryId { get; set; }
        public Country Country { get; set; }
        public ICollection<Album> Albums { get; set; }
    }

    public class Country
    {
        public int CountryId { get; set; }
        public string Name { get; set; }
        public ICollection<Artist> Artists { get; set; }
    }

    public class Album
    {
        public int AlbumId { get; set; }
        public string Name { get; set; }
        public int Year { get; set; }
        public string Genre { get; set; }
        public int ArtistId { get; set; }
        public Artist Artist { get; set; }
        public ICollection<Track> Tracks { get; set; }
    }

    public class Track
    {
        public int TrackId { get; set; }
        public string Name { get; set; }
        public TimeSpan Duration { get; set; }
        public int AlbumId { get; set; }
        public Album Album { get; set; }
        public ICollection<PlaylistTrack> PlaylistTracks { get; set; }
    }

    public class Playlist
    {
        public int PlaylistId { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public ICollection<PlaylistTrack> PlaylistTracks { get; set; }
    }

    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public ICollection<Playlist> Playlists { get; set; }
    }

    public class PlaylistTrack
    {
        public int PlaylistId { get; set; }
        public Playlist Playlist { get; set; }
        public int TrackId { get; set; }
        public Track Track { get; set; }
    }

    public class MusicContext : DbContext
    {
        public DbSet<Artist> Artists { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<Track> Tracks { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=localhost;Database=MusicAppDb;User Id=sa;Password=Anton233@;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PlaylistTrack>().HasKey(pt => new { pt.PlaylistId, pt.TrackId });
        }
    }

    public class MusicContextInitializer : DropCreateDatabaseAlways<MusicContext>
    {
        protected override void Seed(MusicContext context)
        {
            var country = new Country { Name = "USA" };
            context.Countries.Add(country);

            var artist = new Artist { FirstName = "John", LastName = "Doe", Country = country };
            context.Artists.Addcontext.Artists.Add(artist);

            var album = new Album { Name = "Example Album", Year = 2023, Genre = "Rock", Artist = artist };
            context.Albums.Add(album);

            var track1 = new Track { Name = "Example Track 1", Duration = TimeSpan.FromSeconds(180), Album = album };
            var track2 = new Track { Name = "Example Track 2", Duration = TimeSpan.FromSeconds(240), Album = album };
            context.Tracks.AddRange(track1, track2);

            var category = new Category { Name = "Favorites" };
            context.Categories.Add(category);

            context.SaveChanges();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            using (var context = new MusicContext())
            {
                // Инициализация базы данных с данными.
                var initializer = new MusicContextInitializer();
                initializer.InitializeDatabase(context);

                // Создание плейлиста.
                var playlist = new Playlist { Name = "My Playlist", Category = context.Categories.Single(c => c.Name == "Favorites") };
                context.Playlists.Add(playlist);

                // Добавление треков в плейлист.
                var tracks = context.Tracks.ToList();
                foreach (var track in tracks)
                {
                    context.PlaylistTracks.Add(new PlaylistTrack { Playlist = playlist, Track = track });
                }

                context.SaveChanges();

                // Вывод информации о плейлисте и треках.
                Console.WriteLine($"Playlist: {playlist.Name} ({playlist.Category.Name})");
                foreach (var track in playlist.PlaylistTracks.Select(pt => pt.Track))
                {
                    Console.WriteLine($"- {track.Name} ({track.Duration})");
                }
            }
        }
    }
}
