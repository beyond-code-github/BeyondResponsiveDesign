(function (interactions) {

    /***************************************************************************
        Initialise our interactions

    ***************************************************************************/
    interactions.init = function () {

        $.detectSwipe.preventDefault = false;
        $(function () { window.onresize(); });
        window.onresize = function () {
            $(document.body).width(window.innerWidth).height(window.innerHeight);
        }
        touchScrollFix();

        // Default navigation
        interactions.navigate("home");

        /***************************************************************************
            Click handlers

        ***************************************************************************/
        $(window).resize(function (e) {
            interactions.hidePopups(e);
        });
        
        $(document).click(function (e) {
            interactions.hidePopups(e);
        });
        
        $(document.body).on('click', 'header>ul>li.menuToggle', function (e) {
            $(this).trigger('blur');
            e.preventDefault();
            e.stopPropagation();
            interactions.hidePopups(e);
            interactions.toggleSideBar(e);
        });

        $(document.body).on('click', 'header>ul>li.navigator', function (e) {
            $(this).trigger('blur');
            e.preventDefault();
            e.stopPropagation();
            interactions.hidePopups(e);
            interactions.closePropertySheet(e);
        });

        $(document.body).on('click', 'ul>li.navpopup', function (e) {
            e.preventDefault();
            e.stopPropagation();

            if (!$(this).find('.popup').hasClass('active')) {
                interactions.hidePopups(e);
                interactions.showPopup($(this).find('.popup'), this, true);
            } else {
                interactions.hidePopups(e);
            }
        });

        $(document.body).on('click', 'aside.propertySheet', function(e) {
            e.preventDefault();
            e.stopPropagation();
        });

        $(document.body).on('click', '.popup', function (e) {
            e.stopPropagation();
        });

        $(document.body).on('click', '.popup ul>li>a', function (e) {
            e.stopPropagation();
            interactions.hidePopups(e);
        });

        $("main").on('click', ".modalbackground", function (e) {
            interactions.closePropertySheet();
            interactions.hidePopups(e);
        });

        /***************************************************************************
            Swipe handlers

        ***************************************************************************/
        $("main").on('swiperight', function (e) {
            if (!interactions.contentIsTabbed() || interactions.leftmostTabIsOpen()) {
                interactions.showSideBar(e);
            } else {
                interactions.slideTabsLeft();
            }
        });

        $("main").on('swipeleft', function (e) {
            if ($('body').hasClass("sidebar")) {
                interactions.hideSideBar(e);
            } else {
                interactions.slideTabsRight();
            }
        });
    };

    /***************************************************************************
        Sidebar and popup interactions

    ***************************************************************************/
    interactions.toggleSideBar = function (e) {
        if ($('body').hasClass('sidebar')) {
            interactions.hideSideBar(e);
        } else {
            interactions.showSideBar(e);
        }
    };

    interactions.showSideBar = function (e) {
        $('body').addClass('sidebar');
        $(window).trigger('resize');
    };

    interactions.hideSideBar = function (e) {
        $('body').removeClass('sidebar');
        $(window).trigger('resize');
        return false;
    };
    
    interactions.showPopup = function (popup, obj) {
        if (!$(popup).hasClass('active')) {

            $(popup).removeAttr('style');

            var $clone = $(popup).clone(true);
            $clone.addClass('clone');

            var isFixed = (window.getComputedStyle($(popup).get(0), null).getPropertyValue("position") === 'fixed');

            var w = $(obj).outerWidth();
            var pw = $(popup).outerWidth();
            if (pw < w) {
                $(popup).outerWidth(w);
                pw = w;
            }

            var m = $(popup).outerWidth(true) - $(popup).outerWidth(false);
            var posX = Math.round((w - pw - m) / 2);

            var rightViewPortOffset = parseInt($(window).width()) - (parseInt($(obj).offset().left) + parseInt(posX) + parseInt(pw) + parseInt(m));
            if (rightViewPortOffset < 0) {
                posX = (posX + rightViewPortOffset);
            }

            if (!isFixed) {
                $clone.css({ 'left': +($(obj).offset().left + posX - 10) + 'px' });
                $clone.css({ 'top': +($(obj).offset().top + $(obj).height() + 1) + 'px' });
            }

            if (isFixed) {
                $clone.css({ 'max-height': $(popup).outerHeight() + 'px' });
                $clone.find('div.vscroll').css({ 'max-height': Math.round($(window).height() / 2) + 'px' });
                var modalBackground = $('<div class="popupbackground"></div>');
                if ($('div.popupbackground').length == 0) {
                    $('body').prepend(modalBackground);
                    modalBackground.on('touchstart touchmove swipe click', function(e) {
                        interactions.HidePopups(e);
                        e.preventDefault();
                        e.stopPropagation();
                    });
                }
            }
            
            $clone.prependTo('body');
            $clone.css('display', 'block');
            $clone.css('visibility', 'visible');

            setTimeout(function () {
                $(obj).addClass('active');
                $(popup).addClass('active');
                $clone.addClass('active');
            }, 0);
        }
    };

    interactions.hidePopups = function (e) {
        $('.popup').removeClass('active');
        $('.navpopup').removeClass('active');

        $(".clone").remove();
        $('div.popupbackground').remove();
    };
    
    interactions.openPropertySheet = function (location, title) {
        $("body").addClass("propertySheet");

        titleStack.push(title);
        $("#paneTitle").text(title);

        var modalBackground = $('<div class="modalbackground"></div>');
        modalBackground.removeAttr('style');
        modalBackground.prependTo('main');

        $("body > main > aside.propertySheet").empty().removeClass("offRight").addClass("active")
            .on('swiperight', function (e) {
                interactions.closePropertySheet();
                e.stopPropagation();
            });

        return $.get("content/" + location + ".html").done(function (html) {
            var target = $("main > aside");
            target.empty();
            target.html(html);
        });
    }

    interactions.closePropertySheet = function () {
        if ($("body > main > aside.propertySheet").hasClass("active")) {
            $("body").removeClass("propertySheet");

            titleStack.pop();
            $("#paneTitle").text(titleStack[0]);

            $("body > main > .modalbackground").remove();
            $("body > main > aside.propertySheet").off('swiperight').removeClass("active").addClass("offRight");
        }
    }

    /***************************************************************************
        Navigation and tabs

    ***************************************************************************/
    var titleStack = [];
    interactions.navigate = function (location, hideAfter) {
        $.get("content/" + location + ".html").done(function (html) {
            var target = $("main");
            target.empty();
            target.html(html);

            $("nav > div > div > ul  li").removeClass("active");
            var menuitem = $("nav > div > div > ul li#menu-" + location);
            menuitem.addClass("active");

            var title = menuitem.find("a span").first().text();
            titleStack = [title];
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

            if (hideAfter) {
                interactions.hideSideBar();
            }
        });
    }

    interactions.contentIsTabbed = function () {
        return $("article > nav > ul > li.active").length > 0;
    }

    interactions.leftmostTabIsOpen = function () {
        var tab = $("article > nav > ul > li.active");
        return tab.length > 0 && tab.prevAll("li").length == 0;
    }

    interactions.slideTabsLeft = function () {
        var currentTabLink = $("article > nav > ul > li.active");
        var nextTabLink = currentTabLink.prev("li");
        interactions.slideToTab(nextTabLink.attr("id").replace("tab-", ""));
    }

    interactions.slideTabsRight = function () {
        var currentTabLink = $("article > nav > ul > li.active");
        var nextTabLink = currentTabLink.next("li");
        interactions.slideToTab(nextTabLink.attr("id").replace("tab-", ""));
    }

    interactions.slideToTab = function (tab) {
        var tabLink = $("article > nav > ul > li#tab-" + tab);
        shiftElements(tabLink, "li");

        var tabContent = $("article > div.slideTabContainer > section#" + tab);
        shiftElements(tabContent, "section");
    }

    var shiftElements = function (element, siblingSelector) {
        element.siblings().removeClass("active");
        element.prevAll(siblingSelector).removeClass("offRight").addClass("offLeft");
        element.nextAll(siblingSelector).removeClass("offLeft").addClass("offRight");
        element.addClass("active").removeClass("offLeft").removeClass("offRight");
    }

})(window.interactions = window.interactions || {});