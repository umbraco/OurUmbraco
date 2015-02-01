<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [
  <!ENTITY nbsp "&#x00A0;">
]>
<xsl:stylesheet
  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxml="urn:schemas-microsoft-com:xslt"
  xmlns:umbraco.library="urn:umbraco.library"
  xmlns:uForum="urn:uForum"
  exclude-result-prefixes="msxml umbraco.library uForum">


  <xsl:output method="html" omit-xml-declaration="yes"/>

  <xsl:param name="currentPage"/>

  <xsl:template match="/">
    <xsl:variable name="mem" select="uForum:GetCurrentMember()"/>
    <xsl:variable name="editor" select="umbraco.library:NiceUrl(1057)"/>

    <div id="buddyIconForm">
      <a href="javascript:displayOption('twitterSection');" class="iconOption">
        Use your twitter avatar
        <img src="/css/img/icons/twitter.jpg" alt="Twitter bird logo" />
      </a>

      <a href="javascript:displayOption('gravatarSection');" class="iconOption">
        Use gravatar
        <img src="/css/img/icons/gravatar.jpg" alt="Gravatar logo" />
      </a>

      <a href="javascript:displayOption('webcamSection');" class="iconOption">
        Use a webcam picture
        <img src="/css/img/icons/webcam.jpg" alt="Webcam icon" />
      </a>
      <br style="clear: both"/>

      <div id="twitterSection" class="section" style="display: none;">
        <xsl:choose>
          <xsl:when test="string-length($mem/twitter) &gt; 1">
            <h2>Use your twitter image as buddy icon</h2>
            <p>

              <xsl:variable name="avatarUrl" select="concat('//api.twitter.com/1/users/profile_image?screen_name=', $mem/twitter)" />
              <img id="twitterIcon" src="{$avatarUrl}" style="border: 1px solid #ccc;" />
              Use this image as an avatar?
            </p>
            <p>
              <button onclick="setBuddyIconServer('twitter'); killButton(this, 'Done!');">Yes, use this image</button>
            </p>
          </xsl:when>
          <xsl:otherwise>
            <h2>No twitter account found</h2>
            <p>
              You do not have a twitter account registered, please add your twitter alias to your <a href="{$editor}">profile page</a>
            </p>
          </xsl:otherwise>
        </xsl:choose>
      </div>

      <div id="gravatarSection" class="section" style="display: none;">
        <h2>Use gravatar as buddy icon</h2>
        <p>
          <img src="//gravatar.com/avatar/{umbraco.library:md5($mem/@email)}?s=48&amp;d=monsterid" style="border: 1px solid #ccc;" />
          Use this image as an avatar?
        </p>
        <p>
          <button onclick="setBuddyIconServer('gravatar'); killButton(this, 'Done!');">Yes, use this image</button>
        </p>

      </div>

      <div id="webcamSection" class="section" style="display: none;">
        <h2>Use a webcam image as buddy icon</h2>

        <!-- Configure a few settings -->
        <div id="webcamHolder">
          <script language="javascript" type="text/javascript">

            webcam.set_quality(100); // JPEG quality (1 - 100)
            webcam.set_swf_url('/scripts/webcam.swf');
            webcam.set_shutter_sound(false, '/scripts/shutter.mp3'); // play shutter click sound

            webcam.set_api_url('/umbraco/api/Community/SaveWebCamImage/?memberGuid=' + umb_member_guid);

            document.write(webcam.get_html(320, 240, 48, 48));

            webcam.set_hook('onComplete', 'my_completion_handler');
            function my_completion_handler(msg) {
            $("#memberBuddyIcon").css("background-image", "url(" + msg + ")");
            alert("Your buddy icon has changed");
            }

          </script>

          <input type="button" value="Take Snapshot" onClick="webcam.snap()"/>
        </div>

        <p>
          Take a picture of yourself with your webcam and use it as a buddy icon on
          the codegarden website.
        </p>
        <p>
          It's really easy to do:
        </p>

        <ol>
          <li>Make sure your webcam is enabled</li>
          <li>Allow the applet on the right to use your webcam</li>
          <li>
            If you don't see an image to the left, you need to <a href="javascript:webcam.configure();">configure</a> your webcam.
          </li>
          <li>Look pretty</li>
          <li>Press the "Take snapshot" button</li>
        </ol>
      </div>
      <br style="clear: both;"/>

    </div>


    <script type="text/javascript">
      function displayOption(id) {
      $(".section").hide();
      $("#" + id).show();
      }

      function setBuddyIconServer(service){
      $.get("/umbraco/api/Community/SetServiceAsBuddyIcon/?service=" + service, function(data){
      $("#memberAvatar").css("background-image", "url(" + data + ")"); });
      }
    </script>

    <xsl:value-of select="umbraco.library:RegisterJavaScriptFile('webcam.js', '/scripts/webcam.js')"/>


  </xsl:template>

</xsl:stylesheet>