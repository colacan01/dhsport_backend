using System.Text.Json;
using DhSport.Application.DTOs.Post;

namespace DhSport.Application.Common.Helpers;

/// <summary>
/// JSON 객체 비교 헬퍼 클래스
/// </summary>
public static class JsonComparer
{
    /// <summary>
    /// 두 JSON 객체가 동일한지 비교 (key-value 기반)
    /// </summary>
    public static bool AreEqual(object? obj1, object? obj2)
    {
        if (obj1 == null && obj2 == null) return true;
        if (obj1 == null || obj2 == null) return false;

        var json1 = JsonSerializer.Serialize(obj1);
        var json2 = JsonSerializer.Serialize(obj2);

        var element1 = JsonSerializer.Deserialize<JsonElement>(json1);
        var element2 = JsonSerializer.Deserialize<JsonElement>(json2);

        return CompareJsonElements(element1, element2);
    }

    /// <summary>
    /// 두 리비전 간 차이점 추출
    /// </summary>
    public static PostRevisionDiffDto GetDifferences(Guid fromRevisionId, Guid toRevisionId, object? fromContent, object? toContent)
    {
        var diff = new PostRevisionDiffDto
        {
            FromRevisionId = fromRevisionId,
            ToRevisionId = toRevisionId
        };

        if (fromContent == null && toContent == null)
            return diff;

        var fromJson = fromContent != null ? JsonSerializer.Serialize(fromContent) : "{}";
        var toJson = toContent != null ? JsonSerializer.Serialize(toContent) : "{}";

        var fromElement = JsonSerializer.Deserialize<JsonElement>(fromJson);
        var toElement = JsonSerializer.Deserialize<JsonElement>(toJson);

        CompareAndExtractDifferences(fromElement, toElement, "", diff);

        return diff;
    }

    private static bool CompareJsonElements(JsonElement element1, JsonElement element2)
    {
        if (element1.ValueKind != element2.ValueKind)
            return false;

        switch (element1.ValueKind)
        {
            case JsonValueKind.Object:
                var props1 = element1.EnumerateObject().OrderBy(p => p.Name).ToList();
                var props2 = element2.EnumerateObject().OrderBy(p => p.Name).ToList();

                if (props1.Count != props2.Count)
                    return false;

                for (int i = 0; i < props1.Count; i++)
                {
                    if (props1[i].Name != props2[i].Name)
                        return false;

                    if (!CompareJsonElements(props1[i].Value, props2[i].Value))
                        return false;
                }
                return true;

            case JsonValueKind.Array:
                var array1 = element1.EnumerateArray().ToList();
                var array2 = element2.EnumerateArray().ToList();

                if (array1.Count != array2.Count)
                    return false;

                for (int i = 0; i < array1.Count; i++)
                {
                    if (!CompareJsonElements(array1[i], array2[i]))
                        return false;
                }
                return true;

            case JsonValueKind.String:
                return element1.GetString() == element2.GetString();

            case JsonValueKind.Number:
                return element1.GetRawText() == element2.GetRawText();

            case JsonValueKind.True:
            case JsonValueKind.False:
                return element1.GetBoolean() == element2.GetBoolean();

            case JsonValueKind.Null:
                return true;

            default:
                return element1.GetRawText() == element2.GetRawText();
        }
    }

    private static void CompareAndExtractDifferences(
        JsonElement fromElement,
        JsonElement toElement,
        string path,
        PostRevisionDiffDto diff)
    {
        if (fromElement.ValueKind == JsonValueKind.Object && toElement.ValueKind == JsonValueKind.Object)
        {
            var fromProps = fromElement.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);
            var toProps = toElement.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);

            // 추가된 필드
            foreach (var toProp in toProps)
            {
                var currentPath = string.IsNullOrEmpty(path) ? toProp.Key : $"{path}.{toProp.Key}";

                if (!fromProps.ContainsKey(toProp.Key))
                {
                    diff.AddedFields[currentPath] = ConvertJsonElementToObject(toProp.Value);
                }
                else
                {
                    // 재귀적으로 비교
                    if (toProp.Value.ValueKind == JsonValueKind.Object && fromProps[toProp.Key].ValueKind == JsonValueKind.Object)
                    {
                        CompareAndExtractDifferences(fromProps[toProp.Key], toProp.Value, currentPath, diff);
                    }
                    else if (!CompareJsonElements(fromProps[toProp.Key], toProp.Value))
                    {
                        // 수정된 필드
                        diff.ModifiedFields[currentPath] = new ModifiedFieldValue
                        {
                            From = ConvertJsonElementToObject(fromProps[toProp.Key]),
                            To = ConvertJsonElementToObject(toProp.Value)
                        };
                    }
                }
            }

            // 삭제된 필드
            foreach (var fromProp in fromProps)
            {
                if (!toProps.ContainsKey(fromProp.Key))
                {
                    var currentPath = string.IsNullOrEmpty(path) ? fromProp.Key : $"{path}.{fromProp.Key}";
                    diff.RemovedFields[currentPath] = ConvertJsonElementToObject(fromProp.Value);
                }
            }
        }
        else if (!CompareJsonElements(fromElement, toElement))
        {
            // 루트 레벨에서 값이 다른 경우
            diff.ModifiedFields[path] = new ModifiedFieldValue
            {
                From = ConvertJsonElementToObject(fromElement),
                To = ConvertJsonElementToObject(toElement)
            };
        }
    }

    private static object? ConvertJsonElementToObject(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                return element.GetString();
            case JsonValueKind.Number:
                if (element.TryGetInt32(out int intValue))
                    return intValue;
                if (element.TryGetInt64(out long longValue))
                    return longValue;
                if (element.TryGetDouble(out double doubleValue))
                    return doubleValue;
                return element.GetRawText();
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            case JsonValueKind.Null:
                return null;
            case JsonValueKind.Array:
                return element.EnumerateArray().Select(ConvertJsonElementToObject).ToList();
            case JsonValueKind.Object:
                return element.EnumerateObject().ToDictionary(p => p.Name, p => ConvertJsonElementToObject(p.Value));
            default:
                return element.GetRawText();
        }
    }
}
