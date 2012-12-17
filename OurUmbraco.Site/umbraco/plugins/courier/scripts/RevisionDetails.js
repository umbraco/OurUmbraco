var labelType, useGradients, nativeTextSupport, animate;

(function () {
    var ua = navigator.userAgent,
      iStuff = ua.match(/iPhone/i) || ua.match(/iPad/i),
      typeOfCanvas = typeof HTMLCanvasElement,
      nativeCanvasSupport = (typeOfCanvas == 'object' || typeOfCanvas == 'function'),
      textSupport = nativeCanvasSupport
        && (typeof document.createElement('canvas').getContext('2d').fillText == 'function');
    //I'm setting this based on the fact that ExCanvas provides text support for IE
    //and that as of today iPhone/iPad current text support is lame
    labelType = (!nativeCanvasSupport || (textSupport && !iStuff)) ? 'Native' : 'HTML';
    nativeTextSupport = labelType == 'Native';
    useGradients = nativeCanvasSupport;
    animate = !(iStuff || !nativeCanvasSupport);
})();


var Log = {
    elem: false,
    write: function (text) {
        if (!this.elem)
            this.elem = document.getElementById('log');
        //this.elem.innerHTML = text;
        //this.elem.style.left = (500 - this.elem.offsetWidth / 2) + 'px';
    }
};


function init(revisionName) {

    jQuery('#revTree').show();

    jQuery.ajax({
        type: "POST",
        data: '{revisionFolder: "' + revisionName + '"}',
        url: "viewRevision.aspx/RevisionAsJson",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: ajaxCallSucceed,
        failure: fail
    });
    }

    function fail(response) {
       
    }

    function ajaxCallSucceed(response) {

        var json = eval('(' + response.d + ')');
        loadTree(json);

        jQuery("#revTree").css("background-image", "none");
    }

function loadTree(json){

    //end
    //init Spacetree
    //Create a new ST instance
    var st = new $jit.ST({
        //id of viz container element
        injectInto: 'revTree',
        //set duration for the animation
        duration: 800,
        //set animation transition type
        transition: $jit.Trans.Quart.easeInOut,
        //set distance between node and its children
        levelDistance: 100,

        levelsToShow: 4,

        //enable panning
        Navigation: {
            enable: true,
            panning: true
        },
        //set node and edge styles
        //set overridable=true for styling individual
        //nodes or edges
        Node: {
            height: 40,
            width: 100,
            type: 'rectangle',
            color: '#aaa',
            overridable: true
        },


        Edge: {
            type: 'bezier',
            overridable: true
        },

        //This method is called on DOM label creation.
        //Use this method to add event handlers and styles to
        //your node.
        onCreateLabel: function (label, node) {
            label.id = node.id;
            label.innerHTML = node.name;
            label.onclick = function () {
                st.onClick(node.id);
            };

            //set label styles
            var style = label.style;
            style.width = 100 + 'px';
            style.height = 30 + 'px';
            style.cursor = 'pointer';
            style.color = '#000';
            style.fontSize = '10px';
            style.textAlign = 'center';
            style.verticalAlign = 'middle';
            style.padding = '3px';
            style.overflow = 'hidden';
        },


        //This method is called right before plotting
        //a node. It's useful for changing an individual node
        //style properties before plotting it.
        //The data properties prefixed with a dollar
        //sign will override the global node style properties.
        onBeforePlotNode: function (node) {
            //add some color to the nodes in the path between the
            //root node and the selected node.
            if (node.selected) {
                node.data.$color = "#ff7";
            }
        },

        //This method is called right before plotting
        //an edge. It's useful for changing an individual edge
        //style properties before plotting it.
        //Edge data proprties prefixed with a dollar sign will
        //override the Edge global style properties.
        onBeforePlotLine: function (adj) {
            if (adj.nodeFrom.selected && adj.nodeTo.selected) {
                adj.data.$color = "#eed";
                adj.data.$lineWidth = 3;
            }
            else {
                delete adj.data.$color;
                delete adj.data.$lineWidth;
            }
        }
    });
    //load json data
    st.loadJSON(json);


    //compute node positions and layout
    st.compute();

    
    //optional: make a translation of the tree
    //st.geom.translate(new $jit.Complex(-200, 0), "current");

    //emulate a click on the root node.
    st.onClick(st.root);
    //end
    //Add event handlers to switch spacetree orientation.

    /*
    var top = $jit.id('r-top'),
        left = $jit.id('r-left'),
        bottom = $jit.id('r-bottom'),
        right = $jit.id('r-right'),
        normal = $jit.id('s-normal');
        */
    

    function changeHandler() {
        if (this.checked) {
            top.disabled = bottom.disabled = right.disabled = left.disabled = true;
            st.switchPosition(this.value, "animate", {
                onComplete: function () {
                    top.disabled = bottom.disabled = right.disabled = left.disabled = false;
                }
            });
        }
    };

    //top.onchange = left.onchange = bottom.onchange = right.onchange = changeHandler;
    //end

}