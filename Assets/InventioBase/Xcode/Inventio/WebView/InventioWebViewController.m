//
// Sitsim AR Editor, a Unity3D add-on and framework for creating mobile applications with indirect augmented reality.
// Copyright (c) 2019 University of Oslo, CodeGrind AB and CINE (a project under the NPA EU Interreg program)
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
//

#import "InventioWebViewController.h"

#import "InventioViewManager.h"

@interface InventioWebViewController () <UIWebViewDelegate>
@property (retain, nonatomic) IBOutlet UIWebView *webView;
@property (retain, nonatomic) IBOutlet UIBarButtonItem *navigateBackItem;
@property (retain, nonatomic) IBOutlet UIBarButtonItem *navigateForwardItem;
@property (retain, nonatomic) IBOutlet UIActivityIndicatorView *activityIndicator;
@property (retain, nonatomic) NSURL *startURL;

- (void)updateNavigation;
@end

@implementation InventioWebViewController

- (id)initWithNibName:(NSString *)nibNameOrNil bundle:(NSBundle *)nibBundleOrNil
{
    self = [super initWithNibName:nibNameOrNil bundle:nibBundleOrNil];
    if (self) {
        // Custom initialization
    }
    return self;
}

- (void)viewDidLoad
{
    [super viewDidLoad];
    
    self.navigateBackItem.enabled = NO;
    self.navigateForwardItem.enabled = NO;
    
    self.webView.delegate = self;
    
    if (self.startURL) {
        NSURLRequest *request = [NSURLRequest requestWithURL:self.startURL];
        [self.webView loadRequest:request];
    }
}

- (void)viewDidAppear:(BOOL)animated
{
    [super viewDidAppear:animated];
    
    self.view.frame = self.view.superview.bounds;
}

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}

- (void)loadURL:(NSURL *)url
{
    self.startURL = url;
    if (self.startURL) {
        NSURLRequest *request = [NSURLRequest requestWithURL:self.startURL];
        [self.webView loadRequest:request];
    }
}


#pragma mark - Private Interface
- (void)updateNavigation
{
    self.navigateBackItem.enabled = self.webView.canGoBack;
    self.navigateForwardItem.enabled = self.webView.canGoForward;
}


#pragma mark - UI Callbacks
- (IBAction)webViewDone:(UIBarButtonItem *)sender {
    [[InventioViewManager sharedInstance] hideViewController:self];
}


#pragma mark - Web View Delegate
- (void)webView:(UIWebView *)webView didFailLoadWithError:(NSError *)error
{
    [self.activityIndicator stopAnimating];
    [self updateNavigation];
}

- (BOOL)webView:(UIWebView *)webView shouldStartLoadWithRequest:(NSURLRequest *)request
 navigationType:(UIWebViewNavigationType)navigationType
{
    BOOL result = YES;
    if (limitToInitialHost &&
        ([request.URL.scheme isEqualToString:@"http"] || [request.URL.scheme isEqualToString:@"https"])) {
        // Deny loading HTTP URLs outside the initial domain
        NSString *requestHost = request.URL.host;
        NSString *startHost = self.startURL.host;
        
        NSArray *requestComponents = [requestHost componentsSeparatedByString:@"."];
        NSArray *startComponents = [startHost componentsSeparatedByString:@"."];
        if ((requestComponents.count > 2 ) || (startComponents.count > 2)) {
            for (int idx = 1; idx <= 2; ++idx) {
                NSString *requestPart = requestComponents[requestComponents.count - idx];
                NSString *startPart = startComponents[startComponents.count - idx];
                if (![requestPart isEqualToString:startPart]) {
                    result = NO;
                }
            }
        }
        else {
            result = [requestHost isEqualToString:startHost];
        }
    }
    return result;
}

- (void)webViewDidFinishLoad:(UIWebView *)webView
{
    [self.activityIndicator stopAnimating];
    [self updateNavigation];
}

- (void)webViewDidStartLoad:(UIWebView *)webView
{
    [self.activityIndicator startAnimating];
    [self updateNavigation];
}


#pragma mark - Bindings
void _ShowWebViewWithURL(const char *url)
{
    if (url == NULL) {
        return;
    }
    
    InventioWebViewController *controller = [[InventioWebViewController alloc] initWithNibName:@"InventioWebViewController"
                                                                                        bundle:nil];
    NSString *urlString = [NSString stringWithUTF8String:url];
    controller.startURL = [NSURL URLWithString:urlString];
    
    [[InventioViewManager sharedInstance] showViewController:controller
                                             completionBlock:nil];
}

void _ShowWebViewWithPDF(const char *path)
{
    if (path == NULL) {
        return;
    }
    
    InventioWebViewController *controller = [[InventioWebViewController alloc] initWithNibName:@"InventioWebViewController"
                                                                                        bundle:nil];
    NSString *pathString = [NSString stringWithUTF8String:path];
    controller.startURL = [NSURL fileURLWithPath:pathString];
    
    [[InventioViewManager sharedInstance] showViewController:controller
                                             completionBlock:nil];
}

static bool limitToInitialHost = false;
void _LimitWebViewToInitialHost()
{
    limitToInitialHost = true;
}

@end
