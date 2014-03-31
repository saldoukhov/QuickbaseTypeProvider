module Http

open System.Net.Http

let getStringAsync uri (httpClient : HttpClient) = 
    async { 
        let! result = httpClient.GetStringAsync((string) uri) |> Async.AwaitTask
        return result 
        }

let postAsync uri content (httpClient : HttpClient) = 
    async { 
        let! result = httpClient.PostAsync((string) uri, content) |> Async.AwaitTask
        return result 
        }

let sendAsync uri content authorization (httpClient : HttpClient) = 
    async { 
        let request = new HttpRequestMessage(HttpMethod.Post, (string) uri)
        request.Content <- content
        request.Headers.Authorization <- authorization
        let! result = httpClient.SendAsync(request) |> Async.AwaitTask
        return result
    }
