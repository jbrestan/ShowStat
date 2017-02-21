#I "packages/FAKE/tools/"

#r "FakeLib.dll"

open Fake

let repo =
    let repo = getBuildParam "repo"
    if repo = "" then
        failwith """Please specify repository path as 'repo="<path to repository>"'"""
    else repo

let git commandFormat = Printf.kprintf (fun command -> Git.CommandHelper.getGitResult repo command) commandFormat

Target "ShowStat" (fun _ ->
    let getCommits () = git "rev-list HEAD"
    let getFiles commit = git "diff-tree --no-commit-id --name-only -r %s" commit
    let getFiles2 commit = git "diff-tree --no-commit-id --name-only -r %s %i" commit 5

    getCommits ()
    |> Seq.collect getFiles
    |> Seq.countBy id
    |> Seq.sortByDescending snd
    //|> Seq.take 10
    |> Seq.iter (fun (f,c) -> printfn "\t%i\t%s"c f)
)

RunTargetOrDefault "ShowStat"
