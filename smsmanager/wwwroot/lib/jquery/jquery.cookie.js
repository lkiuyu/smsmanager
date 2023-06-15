// jQuery Cookie plugin v1.0
// Enable you to manipulate cookies
// See more: https://github.com/Mandras/jquery.cookie.js

(function ($) {
	$.extend({
		cookie: function (method, object) {
			var parameters = {
				"duration": 30,
				"name": null,
				"value": ""
			};

			if (typeof object === 'undefined') var object = parameters;

			object = $.extend(parameters, object);

			if (typeof method === 'undefined') {
				console.log("Cookie jQuery plugin: Missing method name");
				return (false);
			}

			if (object.name == null || object.name.length == 0) {
				console.log("Cookie jQuery plugin: Missing cookie name");
				return (false);
			}

			// -- Get a cookie

			this.get = function (object) {
				for (var b = object.name + "=", c = document.cookie.split(";"), d = 0; d < c.length; d++) {
					for (var e = c[d]; " " == e.charAt(0);) {
						e = e.substring(1, e.length);
					}
					if (0 == e.indexOf(b)) return (e.substring(b.length, e.length));
				}
				return (null);
			}

			// -- Set a cookie (duration in days):

			this.set = function (object) {
				if (object.duration) {
					var d = new Date;
					d.setTime(d.getTime() + 1e3 * 60 * 60 * 24 * object.duration);
					var e = "; expires=" + d.toGMTString();
				} else var e = "";
				document.cookie = object.name + "=" + object.value + e + "; path=/";
				return (true);
			}

			// -- Delete a cookie:

			this.delete = function (object) {
				$.cookie("set", {
					name: object.name,
					duration: -1,
					value: ""
				});
				return (true);
			}

			// -- Verify if a cookie exist, return `true` or `false`:

			this.exist = function (object) {
				if ($.cookie("get", { name: object.name }) == null) return (false);
				return (true);
			}

			if (typeof (this[method]) === 'function')
				return (this[method](object));

			console.log("Cookie jQuery plugin: Unrecognized method: " + method);
			return (false);
		}
	});
})(jQuery);