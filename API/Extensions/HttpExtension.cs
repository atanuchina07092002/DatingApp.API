using API.Helpers;
using Azure;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json;

namespace API.Extensions
{
    public static class HttpExtension
    {
        public static void AddPaginationHeader<T>(this HttpResponse response, PagedList<T> data)
        {
            var paginationHeader= new PaginationHeader(data.CurrentPage,data.PageSize,data.TotalCount, data.TotalPages);
            var jsonOption= new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };//Converts the header to camelCase JSON
            response.Headers.Append("Pagination",JsonSerializer.Serialize(paginationHeader,jsonOption));
            response.Headers.Append("Access-Control-Expose-Headers", "Pagination");//	Allows browser JS to read the custom "Pagination" header
        }
    }
}
//response.Headers.Append("Pagination", JsonSerializer.Serialize(paginationHeader, jsonOption));
//    Adds a custom header called "Pagination" to the response.
//    The content of the header is a JSON string with info like current page, page size, etc.
//response.Headers.Append("Access-Control-Expose-Headers", "Pagination");
//CORS - related line.
//    By default, browsers don’t allow JavaScript to read custom headers from responses.
//    This line tells the browser: "Hey, it’s okay for client-side JavaScript to access the Pagination header."

