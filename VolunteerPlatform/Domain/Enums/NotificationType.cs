// Domain/Enums/NotificationType.cs

namespace VolunteerPlatform.Domain.Enums;

public enum NotificationType
{
    NewApplication,       // cineva a aplicat la cererea ta
    ApplicationAccepted,  // ai fost acceptat ca voluntar
    ApplicationWithdrawn, // aplicația ta a fost retrasă de celălalt
    RequestInProgress,    // cererea ta a intrat InProgress
    RequestCompleted,     // cererea ta a fost finalizată
    FeedbackReceived,     // ai primit feedback nou
    TicketUpdated,        // actualizare status ticket suport
}