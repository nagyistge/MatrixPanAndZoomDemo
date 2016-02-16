﻿using Perspex;
using Perspex.Controls;
using Perspex.Input;
using Perspex.Media;
using System;

namespace MatrixPanAndZoomDemo.Perspex
{
    public class UIElementZoomManager : Border
    {
        private Control _element;
        private double _zoomSpeed = 1.2;
        private Point _pan;
        private Point _previous;
        private Matrix _matrix = Matrix.Identity;

        public UIElementZoomManager()
            : base()
        {
            Focusable = true;
            Background = Brushes.Transparent;
            DetachedFromVisualTree += UIElementZoomManager_DetachedFromVisualTree;

            ChildProperty.Changed.Subscribe(e =>
            {
                var value = e.NewValue as Control;

                System.Diagnostics.Debug.Print("Border Child Type: " + e.NewValue.GetType());
                if (!(value is Canvas))
                    return;

                if (value != null && value != _element && _element != null)
                {
                    Unload();
                }

                if (value != null && value != _element)
                {
                    Initialize(value);
                }
            });
        }

        private void UIElementZoomManager_DetachedFromVisualTree(object sender, VisualTreeAttachmentEventArgs e)
        {
            if (_element != null)
            {
                Unload();
            }
        }

        private void Initialize(Control element)
        {
            if (element != null)
            {
                _element = element;
                this.Focus();
                this.PointerWheelChanged += Element_PointerWheelChanged;
                this.PointerPressed += Element_PointerPressed;
                this.PointerReleased += Element_PointerReleased;
                this.PointerMoved += Element_PointerMoved;
                this.KeyDown += Element_KeyDown;
            }
        }

        private void Unload()
        {
            if (_element != null)
            {
                this.PointerWheelChanged -= Element_PointerWheelChanged;
                this.PointerPressed -= Element_PointerPressed;
                this.PointerReleased -= Element_PointerReleased;
                this.PointerMoved -= Element_PointerMoved;
                this.KeyDown -= Element_KeyDown;
                _element.RenderTransform = null;
                _element = null;
            }
        }

        private void Invalidate()
        {
            if (_element != null)
            {
                _element.RenderTransform = new MatrixTransform(_matrix);
                _element.InvalidateVisual();
            }
        }

        private void ZoomAsTo(double zoom, Point point)
        {
            _matrix = MatrixHelper.ScaleAtPrepend(_matrix, zoom, zoom, point.X, point.Y);

            Invalidate();
        }

        private void ZoomDeltaTo(double delta, Point point)
        {
            ZoomAsTo(delta > 0 ? _zoomSpeed : 1 / _zoomSpeed, point);
        }

        private void StartPan(Point point)
        {
            _pan = new Point();
            _previous = point;
        }

        private void PanTo(Point point)
        {
            Point delta = point - _previous;
            _previous = point;
            _pan = _pan + delta;
            _matrix = MatrixHelper.TranslatePrepend(_matrix, _pan.X, _pan.Y);

            Invalidate();
        }

        private void Fit()
        {
            if (_element != null)
            {
                double pw = this.Bounds.Width;
                double ph = this.Bounds.Height;
                double ew = _element.Bounds.Width;
                double eh = _element.Bounds.Height;
                double zx = pw / ew;
                double zy = ph / eh;
                double zoom = Math.Min(zx, zy);

                _matrix = MatrixHelper.ScaleAt(zoom, zoom, ew / 2.0, eh / 2.0);

                Invalidate();
            }
        }

        private void Fill()
        {
            if (_element != null)
            {
                double pw = this.Bounds.Width;
                double ph = this.Bounds.Height;
                double ew = _element.Bounds.Width;
                double eh = _element.Bounds.Height;
                double zx = pw / ew;
                double zy = ph / eh;
                
                _matrix = MatrixHelper.ScaleAt(zx, zy, ew / 2.0, eh / 2.0);

                Invalidate();
            }
        }

        private void Reset()
        {
            _matrix = Matrix.Identity;

            Invalidate();
        }

        private void Element_PointerWheelChanged(object sender, PointerWheelEventArgs e)
        {
            if (_element != null)
            {
                Point point = e.GetPosition(_element);
                ZoomDeltaTo(e.Delta.Y, point);
            }
        }

        private bool _captured = false;

        private void Element_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            switch (e.MouseButton)
            {
                case MouseButton.Right:
                    {
                        if (_element != null)
                        {
                            Point point = e.GetPosition(_element);
                            StartPan(point);
                            //e.Device.Capture(_element);
                            _captured = true;
                        }
                    }
                    break;
            }
        }

        private void Element_PointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (_element != null)
            {
                switch (e.MouseButton)
                {
                    case MouseButton.Right:
                        {
                            if (_element != null && _captured == true/*e.Device.Captured == _element*/)
                            {
                                //e.Device.Capture(null);
                                _captured = false;
                            }
                        }
                        break;
                }
            }
        }

        private void Element_PointerMoved(object sender, PointerEventArgs e)
        {
            if (_element != null && _captured == true/*e.Device.Captured == _element*/)
            {
                Point point = e.GetPosition(_element);
                PanTo(point);
            }
        }

        private void Element_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Z)
            {
                Reset();
            }

            if (e.Key == Key.X)
            {
                Fill();
            }
        }
    }
}
