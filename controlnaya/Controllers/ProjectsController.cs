using System.Net;
using System.Text.Encodings.Web;
using Server.Framework;
using Server.Framework.Attributes;
using Server.Framework.HttpResponses;
using Server.Models;
using Server.Services;

namespace Server.Controllers;

[HttpController("projects")]
public sealed class ProjectsController : ControllerBase
{
    private static readonly ProjectService Service = new();

    [Get("")]
    public HtmlResult Index([QueryData] string? technology)
    {
        var projects = Service.GetAll(technology);
        var counts = Service.GetTechnologyCounts();
        string rows = string.Join("", projects.Select(ProjectRow));
        string selected = string.IsNullOrWhiteSpace(technology) ? "Все технологии" : HtmlEncoder.Default.Encode(technology);

        string stats = string.Join("", counts.Select(item =>
            $"<li>{HtmlEncoder.Default.Encode(item.Key)}: {item.Value}</li>"));

        return Html($$$"""
            <!doctype html>
            <html lang="ru">
            <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1">
                <title>Учёт студенческих проектов</title>
                <style>
                    body {{ font-family: Arial, sans-serif; max-width: 1000px; margin: 35px auto; padding: 0 18px; color: #222; }}
                    input, select, button {{ padding: 8px; margin: 4px 0; }}
                    table {{ width: 100%; border-collapse: collapse; margin: 18px 0; }}
                    th, td {{ border: 1px solid #ccc; padding: 9px; text-align: left; }}
                    th {{ background: #f2f2f2; }}
                    section {{ margin: 25px 0; }}
                    .muted {{ color: #666; }}
                </style>
            </head>
            <body>
                <h1>Учёт студенческих проектов</h1>

                <section>
                    <h2>Фильтрация</h2>
                    <form method="get" action="/projects">
                        <label for="technology">Технология:</label>
                        <select id="technology" name="technology">
                            <option value="">Все технологии</option>
                            <option value="csharp">csharp</option>
                            <option value="javascript">javascript</option>
                            <option value="python">python</option>
                        </select>
                        <button type="submit">Фильтровать</button>
                        <a href="/projects">Сбросить</a>
                    </form>
                    <p class="muted">Текущий фильтр: {{{selected}}}</p>
                </section>

                <section>
                    <h2>Создать проект</h2>
                    <form method="post" action="/projects/create">
                        <div><label>Название: <input name="title" minlength="5" maxlength="100" required></label></div>
                        <div>
                            <label>Технология:
                                <select name="technology" required>
                                    <option value="csharp">csharp</option>
                                    <option value="javascript">javascript</option>
                                    <option value="python">python</option>
                                </select>
                            </label>
                        </div>
                        <button type="submit">Создать</button>
                    </form>
                </section>

                <section>
                    <h2>Проекты</h2>
                    <table>
                        <thead>
                            <tr>
                                <th>ID</th>
                                <th>Название</th>
                                <th>Технология</th>
                                <th>Статус</th>
                                <th></th>
                            </tr>
                        </thead>
                        <tbody>{{{rows}}}</tbody>
                    </table>
                    {{{(projects.Count == 0 ? "<p>Проекты не найдены.</p>" : "")}}}
                </section>

                <section>
                    <h2>Количество проектов по технологиям</h2>
                    <ul>{{{stats}}}</ul>
                </section>
            </body>
            </html>
            """);
    }

    [Get("details")]
    public IHttpResponseTypeResult Details([QueryData] int id)
    {
        if (id <= 0)
            return BadRequest("Некорректный идентификатор.");

        Project? project = Service.GetById(id);
        if (project is null)
            return NotFound("Проект не найден.");

        return Html($"""
            <!doctype html>
            <html lang="ru"><head><meta charset="utf-8"><title>Проект #{project.Id}</title></head>
            <body>
                <h1>{HtmlEncoder.Default.Encode(project.Title)}</h1>
                <p>ID: {project.Id}</p>
                <p>Технология: {HtmlEncoder.Default.Encode(project.Technology)}</p>
                <p>Статус: {HtmlEncoder.Default.Encode(project.Status)}</p>
                <p><a href="/projects">К списку проектов</a></p>
            </body></html>
            """);
    }

    [Post("create")]
    public IHttpResponseTypeResult Create([FormData] string title, [FormData] string technology)
    {
        try
        {
            Project project = Service.Create(title, technology);
            return new ResponseResult(string.Empty, 303, location: "/projects");
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [Put("update")]
    public IHttpResponseTypeResult Update([QueryData] int id, [FormData] string title,
        [FormData] string technology, [FormData] string status)
    {
        if (id <= 0)
            return BadRequest("Некорректный идентификатор.");

        try
        {
            if (!Service.Update(id, title, technology, status))
                return NotFound("Проект не найден.");

            return new ResponseResult(string.Empty, 204);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    private static string ProjectRow(Project project) => $"""
        <tr>
            <td>{project.Id}</td>
            <td>{HtmlEncoder.Default.Encode(project.Title)}</td>
            <td>{HtmlEncoder.Default.Encode(project.Technology)}</td>
            <td>{HtmlEncoder.Default.Encode(project.Status)}</td>
            <td><a href="/projects/details?id={project.Id}">Подробнее</a></td>
        </tr>
        """;

    private static ResponseResult BadRequest(string message) =>
        new($"<!doctype html><html lang=\"ru\"><meta charset=\"utf-8\"><h1>400 Bad Request</h1><p>{HtmlEncoder.Default.Encode(message)}</p><a href=\"/projects\">Назад</a></html>",
            400, "text/html; charset=utf-8");

    private static ResponseResult NotFound(string message) =>
        new($"<!doctype html><html lang=\"ru\"><meta charset=\"utf-8\"><h1>404 Not Found</h1><p>{HtmlEncoder.Default.Encode(message)}</p><a href=\"/projects\">Назад</a></html>",
            404, "text/html; charset=utf-8");
}
