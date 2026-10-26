namespace CP5.Application.Common;

public sealed record HateoasLink(
    string Rel,
    string Href,
    string Method);