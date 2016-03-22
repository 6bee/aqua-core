@ECHO OFF
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\msbuild "%~dp0\Aqua.msbuild" /v:minimal /maxcpucount /nodeReuse:false /property:VisualStudioVersion=14.0;Configuration=Release %*
pause