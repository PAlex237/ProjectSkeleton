using System.Collections.Generic;
using System.Diagnostics;
using Silk.NET.SDL;
namespace TheAdventure;

public static class Program
{
    public static void Main()
    {
        var sdl = new Sdl(new SdlContext());

        UInt64 framesRenderedCounter = 0;
        var timer = new Stopwatch();

        ReadOnlySpan<byte> keyboardState;
        unsafe
        {
            keyboardState = new(sdl.GetKeyboardState(null), (int)KeyCode.Count);
        }

        Span<byte> mouseButtonStates = stackalloc byte[(int)MouseButton.Count];

        var ev = new Event();

        var sdlInitResult = sdl.Init(Sdl.InitVideo | Sdl.InitAudio | Sdl.InitEvents | Sdl.InitTimer | Sdl.InitGamecontroller |
                                     Sdl.InitJoystick);
        if (sdlInitResult < 0)
        {
            throw new InvalidOperationException("Failed to initialize SDL.");
        }

        IntPtr window;
        unsafe
        {
            window = (IntPtr)sdl.CreateWindow(
                "The Adventure", Sdl.WindowposUndefined, Sdl.WindowposUndefined, 800, 800,
                (uint)WindowFlags.Resizable | (uint)WindowFlags.AllowHighdpi
            );

            if (window == IntPtr.Zero)
            {
                var ex = sdl.GetErrorAsException();
                if (ex != null)
                {
                    throw ex;
                }

                throw new Exception("Failed to create window.");
            }
        }

        IntPtr renderer;
        unsafe
        {
            renderer = (IntPtr)sdl.CreateRenderer((Window*)window, -1, (uint)RendererFlags.Accelerated);
            sdl.RenderSetVSync((Renderer*)renderer, 1);
        }

        if (renderer == IntPtr.Zero)
        {
            var ex = sdl.GetErrorAsException();
            if (ex != null)
            {
                throw ex;
            }

            throw new Exception("Failed to create renderer.");
        }

        var startX = 100;
        var startY = 100;
        var endX = 200;
        var endY = 200;
        int playerHandX = 100;
        int playerHandY = 500; 
        var playerHand = new List<Card>
        {
            new Card(Suit.Hearts, CardValue.Ten),
            new Card(Suit.Spades, CardValue.Seven),
            new Card(Suit.Clubs, CardValue.Four)
        };
        
        bool quit = false;
       
        while (!quit)
        {
            
            while (sdl.PollEvent(ref ev) != 0)
            {
                if (ev.Type == (uint)EventType.Quit)
                {
                    quit = true;
                    break;
                }

                switch (ev.Type)
                {
                    case (uint)EventType.Windowevent:
                    {
                        switch (ev.Window.Event)
                        {
                            case (byte)WindowEventID.Shown:
                            case (byte)WindowEventID.Exposed:
                            {
                                break;
                            }
                            case (byte)WindowEventID.Hidden:
                            {
                                break;
                            }
                            case (byte)WindowEventID.Moved:
                            {
                                break;
                            }
                            case (byte)WindowEventID.SizeChanged:
                            {
                                break;
                            }
                            case (byte)WindowEventID.Minimized:
                            case (byte)WindowEventID.Maximized:
                            case (byte)WindowEventID.Restored:
                                break;
                            case (byte)WindowEventID.Enter:
                            {
                                break;
                            }
                            case (byte)WindowEventID.Leave:
                            {
                                break;
                            }
                            case (byte)WindowEventID.FocusGained:
                            {
                                break;
                            }
                            case (byte)WindowEventID.FocusLost:
                            {
                                break;
                            }
                            case (byte)WindowEventID.Close:
                            {
                                break;
                            }
                            case (byte)WindowEventID.TakeFocus:
                            {
                                unsafe
                                {
                                    sdl.SetWindowInputFocus(sdl.GetWindowFromID(ev.Window.WindowID));
                                }

                                break;
                            }
                        }

                        break;
                    }

                    case (uint)EventType.Fingermotion:
                    {
                        break;
                    }

                    case (uint)EventType.Mousemotion:
                    {
                        if (keyboardState[(byte)KeyCode.LShift] > 0)
                        {
                            endX = ev.Motion.X;
                            endY = ev.Motion.Y;
                        }
                        else
                        {
                           // startX = ev.Motion.X;
                          //  startY = ev.Motion.Y;
                        }

                        break;
                    }

                    case (uint)EventType.Fingerdown:
                    {
                        mouseButtonStates[(byte)MouseButton.Primary] = 1;
                        break;
                    }
                    case (uint)EventType.Mousebuttondown:
                    {
                        mouseButtonStates[ev.Button.Button] = 1;
                        break;
                    }

                    case (uint)EventType.Fingerup:
                    {
                        mouseButtonStates[(byte)MouseButton.Primary] = 0;
                        break;
                    }

                    case (uint)EventType.Mousebuttonup:
                    {
                        mouseButtonStates[ev.Button.Button] = 0;
                        break;
                    }

                    case (uint)EventType.Mousewheel:
                    {
                        break;
                    }

                    case (uint)EventType.Keyup:
                    {
                        break;
                    }

                    case (uint)EventType.Keydown:
                    {
                        Console.WriteLine($"Key down: {(KeyCode)ev.Key.Keysym.Scancode}");
                        break;
                    }
                }
            }

            var elapsed = timer.Elapsed;
            timer.Restart();

            // game.render(renderer, RenderEvent{ elapsed, framesRenderedCounter++ });
            unsafe
            {
                var r = (Renderer*)renderer;
                sdl.SetRenderDrawColor(r, 0, 100, 0, 255); // Masa de joc
                sdl.RenderClear(r);

                
                for (int i = 0; i < playerHand.Count; i++)
                {
                    // 1. Desenăm dreptunghiul alb (fața cărții)
                    var cardRect = new Silk.NET.Maths.Rectangle<int>(startX + (i * 110), 500, 100, 150);
                    sdl.SetRenderDrawColor(r, 255, 255, 255, 255);
                    sdl.RenderFillRect(r, ref cardRect);

                    // 2. Desenăm conturul negru
                    sdl.SetRenderDrawColor(r, 0, 0, 0, 255);
                    sdl.RenderDrawRect(r, ref cardRect);
                }
                DrawHand(r, sdl, playerHand, playerHandX, playerHandY);
                sdl.RenderPresent(r);
            }

            ++framesRenderedCounter;
        }
        unsafe void DrawHand(Renderer* r, Sdl sdl, List<Card> hand, int startX, int startY)
{
int cardWidth = 70;
int cardHeight = 100;
    int spacing = 10;

    for (int i = 0; i < hand.Count; i++)
    {
        // Calculăm poziția pentru fiecare carte
        var cardRect = new Silk.NET.Maths.Rectangle<int>(startX + i * (cardWidth + spacing), startY, cardWidth, cardHeight);
        
        // Desenăm spatele/fața cărții
        sdl.SetRenderDrawColor(r, 255, 255, 255, 255); // Alb
        sdl.RenderFillRect(r, ref cardRect);
        
        // Desenăm conturul
        sdl.SetRenderDrawColor(r, 0, 0, 0, 255); // Negru
        sdl.RenderDrawRect(r, ref cardRect);
    }
}
        unsafe
        {
            sdl.DestroyWindow((Window*)window);
        }

        sdl.Quit();
    }
}
