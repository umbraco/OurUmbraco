jQuery(function($) {
    var toc = $("#toc ul");
    
  
    jQuery("#toc a.toggle").click(function(){
        toc.toggle();
        if(toc.is(":visible")){
          jQuery("#body").css("margin-right", 280); 
       }else{
          jQuery("#body").css("margin-right", 20);                     
       }
                            
    });
    
  
    $("#body :header").each(function() {
          
          var h = $(this);
          var name = h.text().replace(/[\s,-;\.]/g, "");
          
          h.before( $("<a/>" , {name: name}) );
          
          var li = $("<li class='" + this.tagName + "'></li>");
          var ahref = $("<a/>", {href: "#" + name} ).append( h.text().replace(/\s/g, "&nbsp;") );
          
          li.append(ahref);
          toc.append(li);
    });
  
  
    var parent = toc.parent(),
        tocOverlay = parent
          .clone()
          .insertBefore(parent)
          .css({
            overflow: "visible",
            position: "absolute",
            right: "10px",
        }).hide();
          
});