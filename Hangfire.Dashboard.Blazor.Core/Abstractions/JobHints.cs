using System.Collections.Generic;

namespace Hangfire.Dashboard.Blazor.Core.Abstractions;

public class JobHints
{
    /// <summary>
    /// Названия уникальные типы задач для подсказок для запроса
    /// </summary>
    public IEnumerable<string> Types { get; set; } = [];

    /// <summary>
    /// Названия уникальных методов задач для подсказок для запросов
    /// </summary>
    public IEnumerable<string> Methods { get; set; } = [];
    
    /// <summary>
    /// Названия уникальных состояний задач для подсказок для запросов
    /// </summary>
    public IEnumerable<string> States { get; set; } = [];

    /// <summary>
    /// Названия уникальных очередей для подсказок для запросов
    /// </summary>
    public IEnumerable<string> Queues { get; set; } = [];
    
    /// <summary>
    /// Названия уникальных названий параметров для подсказок в запросе 
    /// </summary>
    public IEnumerable<string> Params { get; set; } = [];

    /// <summary>
    /// Названия уникальных названий аргументов для подсказок в запросе
    /// </summary>
    public IEnumerable<string> ArgNames { get; set; } = [];
}