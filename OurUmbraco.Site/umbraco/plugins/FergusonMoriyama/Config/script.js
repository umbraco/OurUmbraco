var fmo = {};
		fmo.handler = function() {

			var action = $('img:first', this).attr('class');
			var container = $(this).parents("[class=container]")[0];
			var instanceType = $(container).attr('id');
			var instances = $('> .instance', container).size();

			if(action == 'add') {

				$('.containerHeader:first', container).hide();
				var html = $('> .instanceTemplate', container).html();

				var dom = $('> .instanceTemplate', container).clone();

				$('div[class=preInstance]:first', dom).attr('class', 'instance');
				$('div[class=instance] > div', dom).attr('style', '');
				$('div[class=instance] > div[class=containerHeader]', dom).attr('class', 'instanceHeader');
				$('div[class=instance] > div[class=itemTemplate]', dom).attr('class', 'items');

				var thisIstance = instances + 1;
				var thisInstanceId = instanceType+'['+thisIstance+']';

				$('div[class=instance]', dom).attr('id', thisInstanceId);
				$('div[class=instance] > div[class=instanceHeader]', dom).attr('id', instanceType+'['+thisIstance+']/instance');

				$('div[class=instance] > div[class=items] > input', dom).each(function() {
					var id = $(this).attr('id');
					if(id.indexOf('/') > -1) {
						id = id.replace(/^(.*)\/(.*)$/, thisInstanceId + "/$2");
						$(this).attr('id', id);
					} else {
						$(this).attr('id', thisInstanceId + '/'+id);
					}
				});


				var htm = dom.html();
				htm = htm.replace(/display: none/ig, '');


				if($(this).parent().parent().attr('class') == 'instanceHeader') {
					var insertPoint = $(this).parents("[class=instance]")[0];
					$(insertPoint).after(htm);
				} else  {
					$(container).append(htm);
				}

				fmo.renumber(container);

			} else if(action == 'delete') {

				if($(this).parent().parent().attr('class') == 'instanceHeader') {
					var inst = $(this).parents("div[class=instance]")[0];

					$(inst).fadeOut('slow', function() {

						$(inst).remove();
						instances = $('> .instance', container).size();

						if(instances == 0) {
							$('.containerHeader:first', container).show();

						}
						fmo.renumber(container);
					});


				}

			} else if(action == 'up') {
				if(instances > 1) {

					var inst = $(this).parents("div[class=instance]")[0];
					var id  = $(inst).attr('id');

					var pos = id.replace(/.*\[(\d+)\]$/, "$1");
					pos = parseInt(pos);
					if(pos > 1) {

						var target = pos -1;

						var target = $("div[class=instance]", container)[target-1];
						$(inst).swap($(target));

						fmo.renumber(container);
					}

				}
			} else if(action == 'down') {

				var inst = $(this).parents("div[class=instance]")[0];
				var id  = $(inst).attr('id');

				var pos = id.replace(/.*\[(\d+)\]$/, "$1");
				pos = parseInt(pos);

				if(pos < instances) {
					var target = pos +1;
					var target = $("div[class=instance]", container)[target-1];
				    $(inst).swap($(target));
				    fmo.renumber(container);
				}


			}



			$('a').click(fmo.handler);
			return false;
		}

		fmo.renumber = function(container) {

			var id = $(container).attr('id');

			if(id.indexOf('/') > -1) {
				id = id.replace(/^.*\/(.*)$/, "$1");
			}

			var counter = 1;
			$('> .instance', container).each(function() {

				var title = $('> div.instanceHeader > div.title', this);
				var txt = $(title).html();
				txt = txt.replace(/\-\s*\d/, '');
				$(title).html(txt + ' - ' + counter);

				var thisId = $(this).attr('id');

				var pattern = id + '\\[\\d+\\]';
				var re = new RegExp( pattern );

				thisId = thisId.replace(re, id + '[' + counter + ']');

				$(this).attr('id', thisId);

				$('*', this).each(function() {

					if($(this).attr('id').length > 0) {
						var thisId = $(this).attr('id');
						thisId = thisId.replace(re, id + '[' + counter + ']');
						$(this).attr('id', thisId);
					}
				});

				counter++;
			});

		}

		$().ready(function() {

			$('a').click(fmo.handler);

			$("#fmConfigSave").ajaxError(function(event, request, settings){
				alert("Error requesting page: " + settings.url);
				$('#fmConfigSave').attr('disabled', '');
			 	$('#fmConfigSave').val('Save');
			});

			$('#fmConfigSave').click(function() {
				
				var postData = fmo.formXml(new Array($('#Configuration')), 0);
				// postData = escape(postData);
				
				
				var url = $('#fmConfigSaveUrl').val();
				
				$('#fmConfigSave').attr('disabled', 'true');
				$('#fmConfigSave').val('Saving....');
				
				 

				
				$.post(url, { xml: escape(postData) },
				function(data, status){
					
					$('#fmConfigSave').attr('disabled', '');
				 	$('#fmConfigSave').val('Save');
				 	if(status == 'success') {
				 		alert(data);
				 	}
				});

				
				return false;

			});

		});

		fmo.formXml = function(ob, level) {



			var xml = '';

			$(ob).each(function() {

				var type;

				if($(this).hasClass('container')) {
					type = $(this).attr('id');
				} else {

					type = $(this).parents('[class=container]')[0].id;
				}
				type = type.replace(/^.*\/(.*)$/, "$1");

				xml += fmo.tabs(level)+"<"+fmo.xml_escape(type)+">\n";

				$('> input',this).each(function() {
					var n = $(this).attr('id');
					n = n.replace(/^.*\/(.*)$/, "$1");
					xml += fmo.tabs(level+1)+'<'+fmo.xml_escape(n)+'>'+fmo.xml_escape($(this).attr('value'))+'</'+fmo.xml_escape(n)+'>\n';
				});

				var children = $('> .container > .instance > .items', this);

				$('> .container > .instance', this).each(function() {


					// xml += fmo.tabs(level+1)+ '<instance>\n';

					var children = $('> .items', this);

					if($(children).size() > 0)  {
						xml += fmo.formXml(children, level+1);
					}

					// xml += fmo.tabs(level+1) +  '</instance>\n';
				});


				//if($(children).size() > 0) {
				//	xml += fmo.formXml(children, level+1);
				// }

				xml += fmo.tabs(level)+"</"+fmo.xml_escape(type)+">\n";
			});


			return xml;
		};

		fmo.tabs = function(num) {
			var r = '';
			var count = 0;
			while(count < num) {
				r += "\t";
				count++;
			}
			return r;
		}

		fmo.xml_escape = function(s) {
			s = s.replace(/&/g, '&amp;');
			s = s.replace(/</g, '&lt;');
			s = s.replace(/>/g, '&gt;');
			s = s.replace(/'/g, '&apos;');
			s = s.replace(/"/g, '&quot;');
			return s;
		}

		jQuery.fn.swap = function(b) {
		    b = jQuery(b)[0];
		    var a = this[0],
		        a2 = a.cloneNode(true),
		        b2 = b.cloneNode(true),
		        stack = this;

		    a.parentNode.replaceChild(b2, a);
		    b.parentNode.replaceChild(a2, b);

		    stack[0] = a2;
		    return this.pushStack( stack );
		};