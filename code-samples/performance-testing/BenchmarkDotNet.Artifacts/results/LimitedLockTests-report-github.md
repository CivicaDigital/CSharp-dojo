``` ini

BenchmarkDotNet=v0.10.14, OS=Windows 10.0.14393.2189 (1607/AnniversaryUpdate/Redstone1)
Intel Core i7-7600U CPU 2.80GHz (Kaby Lake), 1 CPU, 4 logical and 2 physical cores
Frequency=2835937 Hz, Resolution=352.6171 ns, Timer=TSC
  [Host]     : .NET Framework 4.7.1 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2558.0
  DefaultJob : .NET Framework 4.7.1 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2558.0


```
| Method | ThreadCount |      Mean |     Error |    StdDev |
|------- |------------ |----------:|----------:|----------:|
|  **Tests** |           **2** |  **1.086 ms** | **0.0396 ms** | **0.0389 ms** |
|  **Tests** |           **4** |  **2.084 ms** | **0.0415 ms** | **0.1085 ms** |
|  **Tests** |           **8** |  **6.254 ms** | **0.1245 ms** | **0.1332 ms** |
|  **Tests** |          **16** | **12.614 ms** | **0.2512 ms** | **0.5300 ms** |
|  **Tests** |          **32** | **25.580 ms** | **0.5074 ms** | **0.9776 ms** |
