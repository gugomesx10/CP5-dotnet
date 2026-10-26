using CP5.Application.Common;
using CP5.Application.DTOs.Responses;
using CP5.Application.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CP5.Filters;

public sealed class HateoasResultFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(
        ResultExecutingContext context,
        ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult result)
        {
            switch (result.Value)
            {
                case OperationResult<TimeResumoDto> r
                    when r.Data is not null:
                    AddLinks(r.Data);
                    break;

                case OperationResult<JogadorResumoDto> r
                    when r.Data is not null:
                    AddLinks(r.Data);
                    break;

                case OperationResult<PerfilCompetitivoResponseDto> r
                    when r.Data is not null:
                    AddLinks(r.Data);
                    break;

                case OperationResult<PagedResult<TimeResumoDto>> r
                    when r.Data is not null:
                    EnrichPage(r.Data, context, AddLinks);
                    break;

                case OperationResult<PagedResult<JogadorResumoDto>> r
                    when r.Data is not null:
                    EnrichPage(r.Data, context, AddLinks);
                    break;

                case OperationResult<PagedResult<PerfilCompetitivoResponseDto>> r
                    when r.Data is not null:
                    EnrichPage(r.Data, context, AddLinks);
                    break;
            }
        }

        await next();
    }

    private static void AddLinks(TimeResumoDto time)
    {
        var url = $"/api/Time/{time.Id}";

        time.Links = new List<HateoasLink>
        {
            new("self", url, "GET"),
            new("update", url, "PUT"),
            new("delete", url, "DELETE"),
            new("jogadores", $"/api/Jogador/time/{time.Id}", "GET")
        };
    }

    private static void AddLinks(JogadorResumoDto jogador)
    {
        var url = $"/api/Jogador/{jogador.Id}";

        jogador.Links = new List<HateoasLink>
        {
            new("self", url, "GET"),
            new("update", url, "PUT"),
            new("delete", url, "DELETE"),
            new(
                "perfilCompetitivo",
                $"/api/PerfilCompetitivo/jogador/{jogador.Id}",
                "GET")
        };

        if (jogador.Time is not null)
        {
            AddLinks(jogador.Time);
        }
    }

    private static void AddLinks(PerfilCompetitivoResponseDto perfil)
    {
        var url = $"/api/PerfilCompetitivo/{perfil.Id}";

        perfil.Links = new List<HateoasLink>
        {
            new("self", url, "GET"),
            new("update", url, "PUT"),
            new("delete", url, "DELETE"),
            new("jogador", $"/api/Jogador/{perfil.JogadorId}", "GET")
        };

        if (perfil.Jogador is not null)
        {
            AddLinks(perfil.Jogador);
        }
    }

    private static void EnrichPage<T>(
        PagedResult<T> page,
        ResultExecutingContext context,
        Action<T> addLinks)
    {
        var items = page.Items.ToList();

        foreach (var item in items)
        {
            addLinks(item);
        }

        page.Items = items;

        var path = context.HttpContext.Request.Path.Value ?? "";
        var pageNumber = page.PageNumber;
        var pageSize = page.PageSize;

        string Url(int number) =>
            $"{path}?pageNumber={number}&pageSize={pageSize}";

        page.Links = new List<HateoasLink>
        {
            new("self", Url(pageNumber), "GET")
        };

        if (pageNumber > 1)
        {
            page.Links.Add(
                new HateoasLink("previous", Url(pageNumber - 1), "GET"));
        }

        if (pageNumber < page.TotalPages)
        {
            page.Links.Add(
                new HateoasLink("next", Url(pageNumber + 1), "GET"));
        }
    }
}