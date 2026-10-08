using VolleyPlanner.API.Enums;

namespace VolleyPlanner.API.Entities;

public class TournamentMatch
{
    public int Id { get; set; }

    public int TournamentId { get; set; }

    public int? TournamentPoolId { get; set; }

    public int MatchNumber { get; set; }

    public TournamentMatchStage Stage { get; set; }

    public TournamentMatchStatus Status { get; set; }
        = TournamentMatchStatus.WaitingForTeams;

    // Aktuálisan ismert résztvevők
    public int? Team1EntryId { get; set; }

    public int? Team2EntryId { get; set; }

    // Ha a résztvevő egy előző meccsből érkezik
    public int? Team1SourceMatchId { get; set; }

    public int? Team2SourceMatchId { get; set; }

    public MatchParticipantSourceType Team1SourceType { get; set; }
        = MatchParticipantSourceType.DirectTeam;

    public MatchParticipantSourceType Team2SourceType { get; set; }
        = MatchParticipantSourceType.DirectTeam;

    public int? Team1SetsWon { get; set; }

    public int? Team2SetsWon { get; set; }

    public int? WinnerEntryId { get; set; }

    public int? LoserEntryId { get; set; }

    public bool IsAutomaticResult { get; set; }

    public DateTime? CompletedAt { get; set; }

    public Tournament Tournament { get; set; } = null!;

    public TournamentPool? TournamentPool { get; set; }

    public TournamentEntry? Team1Entry { get; set; }

    public TournamentEntry? Team2Entry { get; set; }

    public TournamentMatch? Team1SourceMatch { get; set; }

    public TournamentMatch? Team2SourceMatch { get; set; }

    public TournamentEntry? WinnerEntry { get; set; }

    public TournamentEntry? LoserEntry { get; set; }

    public ICollection<TournamentMatchSet> Sets { get; set; }
        = new List<TournamentMatchSet>();
}