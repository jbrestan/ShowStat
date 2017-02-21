#I "packages/FAKE/tools/"
#I "packages/Google.DataTable.Net.Wrapper/lib"
#I "packages/XPlot.GoogleCharts/lib/net45"

#r "FakeLib.dll"
#r "Google.DataTable.Net.Wrapper.dll"
#r "XPlot.GoogleCharts.dll"

open System
open Fake
open XPlot.GoogleCharts

let repo =
    let repo = getBuildParam "repo"
    if repo = "" then
        failwith """Please specify repository path as 'repo="<path to repository>"'"""
    else repo

let fileLimit =
    let limitString = getBuildParamOrDefault "limit" "20"
    let success, limit = Int32.TryParse(limitString)
    if (not success) || limit < 0 then
        failwith "File limit must be a positive integer, or 0 if you want to show all files"
    else limit

let git commandFormat = Printf.kprintf (fun command -> Git.CommandHelper.getGitResult repo command) commandFormat

Target "ShowStat" (fun _ ->
    let getCommits () = git "rev-list HEAD"
    let getFiles commit = git "diff-tree --no-commit-id --name-only -r %s" commit

    let limitFiles xs =
        if fileLimit = 0 then xs else Seq.truncate fileLimit xs

    let buildChart (data: seq<string * int>) =
        let opts =
            Options(
                title = "Commits per file",
                vAxis = Axis(title = "Commit count", minValue = 0),
                hAxis = Axis(title = "File name", showTextEvery = 1, slantedText = true))

        data
        |> Seq.map (fun kv -> kv, "Commit count")
        |> List.ofSeq
        |> List.unzip
        |> fun (data, labels) ->
            Chart.Column(data, labels, opts)

    getCommits ()
    |> Seq.collect getFiles
    |> Seq.countBy id
    |> Seq.sortByDescending snd
    |> limitFiles
    |> buildChart
    |> Chart.Show
)

RunTargetOrDefault "ShowStat"
