(function (interactions) {

    interactions.init = function () {
        $.detectSwipe.preventDefault = false;
        interactions.navigate("home");
        
        // Disable scroll for the document, we'll handle it ourselves
        $(document).on('touchmove', function (e) {
            e.preventDefault();
        });

        // Check if we should allow scrolling up or down
        $(document.body).on("touchstart", ".vscroll", function (e) {
            // If the element is scrollable (content overflows), then...
            if (this.scrollHeight !== this.clientHeight) {
                // If we're at the top, scroll down one pixel to allow scrolling up
                if (this.scrollTop === 0) {
                    this.scrollTop = 1;
                }
                // If we're at the bottom, scroll up one pixel to allow scrolling down
                if (this.scrollTop === this.scrollHeight - this.clientHeight) {
                    this.scrollTop = this.scrollHeight - this.clientHeight - 1;
                }
            }
            // Check if we can scroll
            this.allowUp = this.scrollTop > 0;
            this.allowDown = this.scrollTop < (this.scrollHeight - this.clientHeight);
            this.lastY = e.originalEvent.pageY;
        });

        $(document.body).on('touchmove', ".vscroll", function (e) {
            var event = e.originalEvent;
            var up = event.pageY > this.lastY;
            var down = !up;
            this.lastY = event.pageY;

            if ((up && this.allowUp) || (down && this.allowDown)) {
                event.stopPropagation();
            } else {
                event.preventDefault();
            }
        });
        
        window.onresize = function () {
            $(document.body).width(window.innerWidth).height(window.innerHeight);
        }

        $(function () {
            window.onresize();
        });

        $(document.body).on('click', 'header>ul>li.menuToggle', function (e) {
            $(this).trigger('blur');
            e.preventDefault();
            e.stopPropagation();
            interactions.HidePopups(e);
            interactions.ToggleSideBar(e);
        });
        
        $("main").on('swiperight', function (e) {
            if (!interactions.contentIsTabbed() || interactions.leftmostTabIsOpen()) {
                interactions.ShowSideBar(e);
            } else {
                interactions.slideTabsLeft();
            }
        });

        $("main").on('swipeleft', function (e) {
            if ($('body').hasClass("sidebar")) {
                interactions.HideSideBar(e);
            } else {
                interactions.slideTabsRight();
            }
        });
    };

    interactions.ToggleSideBar = function (e) {
        if ($('body').hasClass('sidebar')) {
            interactions.HideSideBar(e);
        } else {
            interactions.ShowSideBar(e);
        }
    };

    interactions.ShowSideBar = function (e) {
        $('body').addClass('sidebar');
        $(window).trigger('resize');
    };

    interactions.HideSideBar = function (e) {
        $('body').removeClass('sidebar');
        $(window).trigger('resize');
        return false;
    };

    interactions.HidePopups = function (e) {

    };

    interactions.navigate = function (location) {
        $.get("content/" + location + ".html").done(function (html) {
            var target = $("main");
            target.empty();
            target.html(html);

            $("nav > div > div > ul  li").removeClass("active");
            var menuitem = $("nav > div > div > ul li#menu-" + location);
            menuitem.addClass("active");

            var title = menuitem.find("a span").first().text();
            $("#paneTitle").text(title);

            var parentDiv = menuitem.parents("div").first();
            var selected = null;
            if (parentDiv.hasClass("wrapper")) {
                selected = menuitem.children("li").first();
            } else {
                selected = parentDiv.parent();
            }

            if (selected) {
                selected.addClass("active");
            }
        });
    }

    interactions.contentIsTabbed = function() {
        return $("article > nav > ul > li.active").length > 0;
    }

    interactions.leftmostTabIsOpen = function () {
        var tab = $("article > nav > ul > li.active");
        return tab.length > 0 && tab.prevAll("li").length == 0;
    }

    interactions.slideTabsLeft = function () {
        var currentTabLink = $("article > nav > ul > li.active");
        var nextTabLink = currentTabLink.prev("li");
        interactions.slideToTab(nextTabLink.attr("id").replace("tab-",""));
    }

    interactions.slideTabsRight = function () {
        var currentTabLink = $("article > nav > ul > li.active");
        var nextTabLink = currentTabLink.next("li");
        interactions.slideToTab(nextTabLink.attr("id").replace("tab-",""));
    }

    var shiftElements = function(element, siblingSelector) {
        element.siblings().removeClass("active");
        element.prevAll(siblingSelector).removeClass("offRight");
        element.prevAll(siblingSelector).addClass("offLeft");

        element.nextAll(siblingSelector).removeClass("offLeft");
        element.nextAll(siblingSelector).addClass("offRight");

        element.addClass("active");
        element.removeClass("offLeft");
        element.removeClass("offRight");
    }

    interactions.slideToTab = function(tab) {
        var tabLink = $("article > nav > ul > li#tab-" + tab);
        shiftElements(tabLink, "li");

        var tabContent = $("article > div.slideTabContainer > section#" + tab);
        shiftElements(tabContent, "section");
    }

})(window.interactions = window.interactions || {});