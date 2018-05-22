``` ini

BenchmarkDotNet=v0.10.14, OS=Windows 10.0.14393.2189 (1607/AnniversaryUpdate/Redstone1)
Intel Core i7-7600U CPU 2.80GHz (Kaby Lake), 1 CPU, 4 logical and 2 physical cores
Frequency=2835937 Hz, Resolution=352.6171 ns, Timer=TSC
  [Host]     : .NET Framework 4.7.1 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2558.0
  DefaultJob : .NET Framework 4.7.1 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2558.0


```
| Method | ThreadCount |      Mean |     Error |    StdDev |
|------- |------------ |----------:|----------:|----------:|
|  **Tests** |           **2** |  **1.326 ms** | **0.0256 ms** | **0.0295 ms** |
|  **Tests** |           **4** |  **2.926 ms** | **0.0576 ms** | **0.1009 ms** |
|  **Tests** |           **8** |  **7.601 ms** | **0.1481 ms** | **0.2170 ms** |
|  **Tests** |          **16** | **14.958 ms** | **0.2926 ms** | **0.4102 ms** |
|  **Tests** |          **32** | **31.122 ms** | **0.6139 ms** | **0.8805 ms** |
