``` ini

BenchmarkDotNet=v0.10.14, OS=Windows 10.0.14393.2189 (1607/AnniversaryUpdate/Redstone1)
Intel Core i7-7600U CPU 2.80GHz (Kaby Lake), 1 CPU, 4 logical and 2 physical cores
Frequency=2835937 Hz, Resolution=352.6171 ns, Timer=TSC
  [Host]     : .NET Framework 4.7.1 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2558.0
  DefaultJob : .NET Framework 4.7.1 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2558.0


```
| Method | ThreadCount |      Mean |     Error |    StdDev |
|------- |------------ |----------:|----------:|----------:|
|  **Tests** |           **2** |  **1.192 ms** | **0.0238 ms** | **0.0697 ms** |
|  **Tests** |           **4** |  **2.177 ms** | **0.0435 ms** | **0.0637 ms** |
|  **Tests** |           **8** |  **6.429 ms** | **0.1264 ms** | **0.1456 ms** |
|  **Tests** |          **16** | **12.913 ms** | **0.2547 ms** | **0.5696 ms** |
|  **Tests** |          **32** | **26.000 ms** | **0.5180 ms** | **0.9340 ms** |
