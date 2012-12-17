REM Following line in original script incorrectly sets all child folder permissions
REM icacls . /grant "IIS APPPOOL\our.umbraco.org.live":(OI)(CI)M
icacls app_code /grant "IIS APPPOOL\our.umbraco.org.live":(OI)(CI)RX
icacls app_browsers /grant "IIS APPPOOL\our.umbraco.org.live":(OI)(CI)RX
icacls app_data /grant "IIS APPPOOL\our.umbraco.org.live":(OI)(CI)M
icacls bin /grant "IIS APPPOOL\our.umbraco.org.live":(OI)(CI)R
icacls macroScripts /grant "IIS APPPOOL\our.umbraco.org.live":(OI)(CI)M
icacls upowers /grant "IIS APPPOOL\our.umbraco.org.live":(OI)(CI)M
icacls config /grant "IIS APPPOOL\our.umbraco.org.live":(OI)(CI)M
icacls css /grant "IIS APPPOOL\our.umbraco.org.live":(OI)(CI)M
icacls data /grant "IIS APPPOOL\our.umbraco.org.live":(OI)(CI)M
icacls masterpages /grant "IIS APPPOOL\our.umbraco.org.live":(OI)(CI)M
icacls media /grant "IIS APPPOOL\our.umbraco.org.live":(OI)(CI)M
icacls python /grant "IIS APPPOOL\our.umbraco.org.live":(OI)(CI)M
icacls scripts /grant "IIS APPPOOL\our.umbraco.org.live":(OI)(CI)M
icacls umbraco /grant "IIS APPPOOL\our.umbraco.org.live":(OI)(CI)R
icacls usercontrols /grant "IIS APPPOOL\our.umbraco.org.live":(OI)(CI)R
icacls xslt /grant "IIS APPPOOL\our.umbraco.org.live":(OI)(CI)M
icacls web.config /grant "IIS APPPOOL\our.umbraco.org.live":(OI)(CI)M
icacls web.config /grant "IIS APPPOOL\our.umbraco.org.live":M
REM If you have installed the Robots.txt editor package you need the following line too
icacls robots.txt /grant "IIS APPPOOL\our.umbraco.org.live":M