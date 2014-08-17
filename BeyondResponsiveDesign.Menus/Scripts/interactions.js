(function (interactions) {

    interactions.init = function () {
        $.detectSwipe.preventDefault = false;
        interactions.navigate("home");
        
        $('body').on('touchmove', function (e) {
            var searchTerms = '.vscroll, .vscroll-scroll, .hscroll',
                $target = $(e.target),
                parents = $target.parents(searchTerms);

            if (parents.length || $target.hasClass(searchTerms)) {
                // ignore as we want the scroll to happen
                // (This is where we may need to check if at limit)
            } else {
                e.preventDefault();
            }
        });

        window.onresize = function () {
            $(document.body).width(window.innerWidth).height(window.innerHeight);
        }

        $(function () {
            window.onresize();
        });

        $(document.body).on('click', 'nav>ul>li.menuToggle', function (e) {
            $(this).trigger('blur');
            e.preventDefault();
            e.stopPropagation();
            interactions.HidePopups(e);
            interactions.ToggleSideBar(e);
        });
        
        $("article").on('swiperight', function (e) {
            if (!interactions.contentIsTabbed() || interactions.leftmostTabIsOpen()) {
                interactions.ShowSideBar(e);
            } else {
                interactions.slideTabsLeft();
            }
        });

        $("article").on('swipeleft', function (e) {
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
            var target = $("section.target");
            target.empty();
            target.html(html);

            $("article > aside > nav > div > div > ul  li").removeClass("active");
            var menuitem = $("article > aside > nav > div > div > ul li#menu-" + location);
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
        return $("article > section > nav > ul > li.active").length > 0;
    }

    interactions.leftmostTabIsOpen = function () {
        var tab = $("article > section > nav > ul > li.active");
        return tab.length > 0 && tab.prevAll("li").length == 0;
    }

    interactions.slideTabsLeft = function () {
        var currentTabLink = $("article > section > nav > ul > li.active");
        var nextTabLink = currentTabLink.prev("li");
        interactions.slideToTab(nextTabLink.attr("id").replace("tab-",""));
    }

    interactions.slideTabsRight = function () {
        var currentTabLink = $("article > section > nav > ul > li.active");
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
        var tabLink = $("article > section > nav > ul > li#tab-" + tab);
        shiftElements(tabLink, "li");

        var tabContent = $("article > section > section.content > div#" + tab);
        shiftElements(tabContent, "div");
    }

})(window.interactions = window.interactions || {});