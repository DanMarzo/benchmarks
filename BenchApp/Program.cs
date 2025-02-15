using BenchApp;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<ExecuteBench>();

//var tes = new ExecuteBench();

//await tes.GetPessoasWithDapper();

Console.ReadKey();