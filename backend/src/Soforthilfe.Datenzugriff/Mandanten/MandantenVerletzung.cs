namespace Soforthilfe.Datenzugriff.Mandanten;

/// <summary>
/// Wird ausgelöst, wenn ein Zugriff die Grenze der eigenen Region überschreiten würde.
/// </summary>
public sealed class MandantenVerletzung(string meldung) : InvalidOperationException(meldung);
