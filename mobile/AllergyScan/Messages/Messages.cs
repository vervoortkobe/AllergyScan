using AllergyScan.Models;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AllergyScan.Messages
{
    /// <summary>Sent when the user opens a scan from the history list.</summary>
    public sealed class ScanSelectedMessage(ScanHistory scan) : ValueChangedMessage<ScanHistory>(scan)
    {
    }

    /// <summary>Sent after a scan was created, changed or deleted, so the history can reload.</summary>
    public sealed class RefreshScansMessage
    {
    }

    /// <summary>Sent when the user selects another translation language.</summary>
    public sealed class LanguageChangedMessage(Language language) : ValueChangedMessage<Language>(language)
    {
    }
}
