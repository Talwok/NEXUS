using System.Collections.Concurrent;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.Utilities;
using FluentAvalonia.UI.Controls;

namespace NEXUS.Styles.Control.Custom.RangeSlider;

public partial class NexusRangeSlider : TemplatedControl
{
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<NexusRangeSlider, Orientation>(nameof(Orientation), Orientation.Horizontal);

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == RangeStartProperty)
        {
            _minSet = true;

            if (!_valuesAssigned)
                return;

            var newV = change.GetNewValue<double>();
            RangeMinToStepFrequency();

            if (_valuesAssigned)
            {
                if (newV < Minimum)
                    RangeStart = Minimum;
                else if (newV > Maximum)
                    RangeStart = Maximum;

                SyncActiveRectangle();

                if (newV > RangeEnd)
                    RangeEnd = newV;
            }

            SyncThumbs();

            if (!_isDraggingEnd && !_isDraggingStart)
            {
                OnValueChanged(new RangeChangedEventArgs(change.GetOldValue<double>(), newV,
                    RangeSelectorProperty.RangeStartValue));
            }
        }
        else if (change.Property == RangeEndProperty)
        {
            _maxSet = true;

            if (!_valuesAssigned)
                return;

            var newV = change.GetNewValue<double>();
            RangeMaxToStepFrequency();

            if (_valuesAssigned)
            {
                if (newV < Minimum)
                    RangeEnd = Minimum;
                else if (newV > Maximum)
                    RangeEnd = Maximum;

                SyncActiveRectangle();

                if (newV < RangeStart)
                    RangeStart = newV;
            }

            SyncThumbs();

            if (!_isDraggingEnd && !_isDraggingStart)
            {
                OnValueChanged(new RangeChangedEventArgs(change.GetOldValue<double>(), newV,
                    RangeSelectorProperty.RangeEndValue));
            }
        }
        else if (change.Property == MinimumProperty)
        {
            if (!_valuesAssigned)
                return;

            var (oldV, newV) = change.GetOldAndNewValue<double>();

            if (Maximum < newV)
                Maximum = newV + Epsilon;

            if (RangeStart < newV)
                RangeStart = newV;

            if (RangeEnd < newV)
                RangeEnd = newV;

            if (newV != oldV)
                SyncThumbs();
        }
        else if (change.Property == MaximumProperty)
        {
            if (!_valuesAssigned)
                return;

            var (oldV, newV) = change.GetOldAndNewValue<double>();

            if (Minimum > newV)
                Maximum = newV + Epsilon;

            if (RangeEnd > newV)
                RangeEnd = newV;

            if (RangeStart > newV)
                RangeStart = newV;

            if (newV != oldV)
                SyncThumbs();
        }
        else if (change.Property == OrientationProperty)
        {
            SyncThumbs();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_minThumb != null)
        {
            _minThumb.DragCompleted -= HandleThumbDragCompleted;
            _minThumb.DragDelta -= MinThumbDragDelta;
            _minThumb.DragStarted -= MinThumbDragStarted;
            _minThumb.KeyDown -= MinThumbKeyDown;
            _minThumb.KeyUp -= ThumbKeyUp;

            _maxThumb.DragCompleted -= HandleThumbDragCompleted;
            _maxThumb.DragDelta -= MaxThumbDragDelta;
            _maxThumb.DragStarted -= MaxThumbDragStarted;
            _maxThumb.KeyDown -= MaxThumbKeyDown;
            _maxThumb.KeyUp -= ThumbKeyUp;

            _containerCanvas.SizeChanged -= ContainerCanvasSizeChanged;
            _containerCanvas.PointerPressed -= ContainerCanvasPointerPressed;
            _containerCanvas.PointerMoved -= ContainerCanvasPointerMoved;
            _containerCanvas.PointerReleased -= ContainerCanvasPointerReleased;
            _containerCanvas.PointerExited -= ContainerCanvasPointerExited;
        }

        base.OnApplyTemplate(e);

        VerifyValues();
        _valuesAssigned = true;

        _activeRectangle = e.NameScope.Get<Rectangle>(s_tpActiveRectangle);
        _minThumb = e.NameScope.Get<Thumb>(s_tpMinThumb);
        _maxThumb = e.NameScope.Get<Thumb>(s_tpMaxThumb);
        _containerCanvas = e.NameScope.Get<Canvas>(s_tpContainerCanvas);
        _toolTip = e.NameScope.Find<Avalonia.Controls.Control>("ToolTip");
        _toolTipText = e.NameScope.Find<TextBlock>(s_tpToolTipText);

        if (_toolTip != null)
        {
            if (_toolTip.Parent is Panel p)
                p.Children.Remove(_toolTip);
        }

        _minThumb.DragCompleted += HandleThumbDragCompleted;
        _minThumb.DragDelta += MinThumbDragDelta;
        _minThumb.DragStarted += MinThumbDragStarted;
        _minThumb.KeyDown += MinThumbKeyDown;
        _minThumb.KeyUp += ThumbKeyUp;

        _maxThumb.DragCompleted += HandleThumbDragCompleted;
        _maxThumb.DragDelta += MaxThumbDragDelta;
        _maxThumb.DragStarted += MaxThumbDragStarted;
        _maxThumb.KeyDown += MaxThumbKeyDown;
        _maxThumb.KeyUp += ThumbKeyUp;

        _containerCanvas.SizeChanged += ContainerCanvasSizeChanged;
        _containerCanvas.PointerPressed += ContainerCanvasPointerPressed;
        _containerCanvas.PointerMoved += ContainerCanvasPointerMoved;
        _containerCanvas.PointerReleased += ContainerCanvasPointerReleased;
        _containerCanvas.PointerExited += ContainerCanvasPointerExited;
    }

    private void MinThumbDragDelta(object sender, VectorEventArgs e)
    {
        if (Orientation == Orientation.Horizontal)
        {
            _absolutePosition += e.Vector.X;
            var oldStart = RangeStart;
            var newStart = DragThumb(_minThumb, 0, DragWidth, _absolutePosition);

            var limit = RangeEnd - MinimumRange;
            if (newStart > limit)
            {
                RangeEnd += newStart - oldStart;
                newStart -= newStart - limit;
                RangeStart = newStart;
            }
            else
            {
                RangeStart = newStart;
            }
        }
        else
        {
            _absolutePosition += e.Vector.Y;
            var oldStart = RangeStart;
            var newStart = DragThumb(_minThumb, 0, DragHeight, _absolutePosition, true);

            var limit = RangeEnd - MinimumRange;
            if (newStart > limit)
            {
                RangeEnd += newStart - oldStart;
                newStart -= newStart - limit;
                RangeStart = newStart;
            }
            else
            {
                RangeStart = newStart;
            }
        }

        if (_toolTipText != null)
        {
            UpdateToolTipText(RangeStart);
        }
    }

    private void MaxThumbDragDelta(object sender, VectorEventArgs e)
    {
        if (Orientation == Orientation.Horizontal)
        {
            _absolutePosition += e.Vector.X;
            var oldEnd = RangeEnd;
            var newEnd = DragThumb(_maxThumb, 0, DragWidth, _absolutePosition);

            var limit = RangeStart + MinimumRange;
            if (newEnd < limit)
            {
                RangeStart -= oldEnd - newEnd;
                newEnd -= newEnd - limit;
                RangeEnd = newEnd;
            }
            else
            {
                RangeEnd = newEnd;
            }
        }
        else
        {
            _absolutePosition += e.Vector.Y;
            var oldEnd = RangeEnd;
            var newEnd = DragThumb(_maxThumb, 0, DragHeight, _absolutePosition, true);

            var limit = RangeStart + MinimumRange;
            if (newEnd < limit)
            {
                RangeStart -= oldEnd - newEnd;
                newEnd -= newEnd - limit;
                RangeEnd = newEnd;
            }
            else
            {
                RangeEnd = newEnd;
            }
        }

        if (_toolTipText != null)
        {
            UpdateToolTipText(RangeEnd);
        }
    }

    private double DragThumb(Thumb thumb, double min, double max, double nextPos, bool isVertical = false)
    {
        nextPos = Math.Max(min, nextPos);
        nextPos = Math.Min(max, nextPos);

        if (isVertical)
        {
            Canvas.SetTop(thumb, max - nextPos); // Инвертируем для вертикального движения
        }
        else
        {
            Canvas.SetLeft(thumb, nextPos);
        }

        return Minimum + ((nextPos / (isVertical ? DragHeight : DragWidth)) * (Maximum - Minimum));
    }
    
    private void SyncActiveRectangle()
    {
        if (_containerCanvas == null || _minThumb == null || _maxThumb == null)
            return;

        if (Orientation == Orientation.Horizontal)
        {
            var relativeLeft = Canvas.GetLeft(_minThumb);
            Canvas.SetLeft(_activeRectangle, relativeLeft);
            Canvas.SetTop(_activeRectangle, (_containerCanvas.Bounds.Height - _activeRectangle.Bounds.Height) / 2);
            _activeRectangle.Width = Math.Max(0, Canvas.GetLeft(_maxThumb) - Canvas.GetLeft(_minThumb));
            _activeRectangle.Height = 2;
        }
        else
        {
            var relativeTop = Canvas.GetTop(_maxThumb);
            Canvas.SetTop(_activeRectangle, relativeTop);
            Canvas.SetLeft(_activeRectangle, (_containerCanvas.Bounds.Width - _activeRectangle.Bounds.Width) / 2);
            _activeRectangle.Height = Math.Max(0, Canvas.GetTop(_minThumb) - Canvas.GetTop(_maxThumb));
            _activeRectangle.Width = 2;
        }
    }

    // Остальные методы остаются без изменений, кроме тех, что работают с координатами
    // (ContainerCanvasPointerPressed, ContainerCanvasPointerMoved и т.д.)
    // Их нужно аналогично модифицировать для поддержки вертикальной ориентации
    private double _dragWidth;
    private double _dragHeight;

    private double DragWidth
    {
        get => _dragWidth;
        set => _dragWidth = value;
    }

    private double DragHeight
    {
        get => _dragHeight;
        set => _dragHeight = value;
    }

    private void SyncThumbs(bool fromMinKeyDown = false, bool fromMaxKeyDown = false)
    {
        if (_containerCanvas == null)
            return;

        // Обновляем размеры области перетаскивания
        DragWidth = _containerCanvas.Bounds.Width;
        DragHeight = _containerCanvas.Bounds.Height;

        double relativeMin = ((RangeStart - Minimum) / (Maximum - Minimum)) *
                             (Orientation == Orientation.Horizontal ? DragWidth : DragHeight);
        double relativeMax = ((RangeEnd - Minimum) / (Maximum - Minimum)) *
                             (Orientation == Orientation.Horizontal ? DragWidth : DragHeight);

        if (Orientation == Orientation.Horizontal)
        {
            Canvas.SetLeft(_minThumb, relativeMin);
            Canvas.SetLeft(_maxThumb, relativeMax);

            var y = _containerCanvas.Bounds.Height / 2 - _minThumb.Bounds.Height / 2;
            Canvas.SetTop(_minThumb, y);
            Canvas.SetTop(_maxThumb, y);
        }
        else
        {
            // Для вертикального режима инвертируем координаты (0 внизу)
            Canvas.SetTop(_minThumb, DragHeight - relativeMax);
            Canvas.SetTop(_maxThumb, DragHeight - relativeMin);

            var x = _containerCanvas.Bounds.Width / 2 - _minThumb.Bounds.Width / 2;
            Canvas.SetLeft(_minThumb, x);
            Canvas.SetLeft(_maxThumb, x);
        }

        // Обновляем абсолютную позицию для текущего перетаскивания
        if (_isDraggingStart)
        {
            _absolutePosition = Orientation == Orientation.Horizontal ? relativeMin : relativeMax;
        }
        else if (_isDraggingEnd)
        {
            _absolutePosition = Orientation == Orientation.Horizontal ? relativeMax : relativeMin;
        }

        if (fromMinKeyDown || fromMaxKeyDown)
        {
            var thumb = fromMinKeyDown ? _minThumb : _maxThumb;
            var pos = fromMinKeyDown ? relativeMin : relativeMax;

            if (Orientation == Orientation.Horizontal)
            {
                Canvas.SetLeft(thumb, pos);
            }
            else
            {
                Canvas.SetTop(thumb, DragHeight - pos);
            }

            if (_toolTipText != null)
            {
                UpdateToolTipText(fromMinKeyDown ? RangeStart : RangeEnd);
            }
        }

        SyncActiveRectangle();
    }

    private void ContainerCanvasPointerPressed(object sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(_containerCanvas);
        var position = Orientation == Orientation.Horizontal ? point.Position.X : point.Position.Y;
        var size = Orientation == Orientation.Horizontal ? DragWidth : DragHeight;

        var mods = TopLevel.GetTopLevel(this).PlatformSettings.HotkeyConfiguration.CommandModifiers;
        if (mods == KeyModifiers.None)
            mods = KeyModifiers.Control;

        if ((e.KeyModifiers & mods) == mods)
        {
            _pointerManipulatingBoth = true;
            _absolutePosition = position;
            return;
        }

        var normalizedPosition = (position / size) * (Maximum - Minimum) + Minimum;
        double upperValueDiff = Math.Abs(RangeEnd - normalizedPosition);
        double lowerValueDiff = Math.Abs(RangeStart - normalizedPosition);

        if (upperValueDiff < lowerValueDiff)
        {
            RangeEnd = normalizedPosition;
            _pointerManipulatingMax = true;
            HandleThumbDragStarted(_maxThumb);
        }
        else
        {
            RangeStart = normalizedPosition;
            _pointerManipulatingMin = true;
            HandleThumbDragStarted(_minThumb);
        }

        SyncThumbs();
    }

    private void ContainerCanvasPointerMoved(object sender, PointerEventArgs e)
    {
        var point = e.GetCurrentPoint(_containerCanvas);
        var position = Orientation == Orientation.Horizontal ? point.Position.X : point.Position.Y;
        var size = Orientation == Orientation.Horizontal ? DragWidth : DragHeight;

        if (_pointerManipulatingBoth)
        {
            var max = Maximum;
            var min = Minimum;
            var dragDelta = position - _absolutePosition;
            var delta = ((dragDelta / size) * (max - min));

            if (Math.Abs(delta) < StepFrequency)
                return;

            var rs = RangeStart;
            var re = RangeEnd;

            if (delta > 0)
            {
                if (MathUtilities.AreClose(re, max))
                    return;

                if (re + delta > max)
                    delta = max - re;
            }
            else if (delta < 0)
            {
                if (MathUtilities.AreClose(rs, min))
                    return;

                if (rs + delta < min)
                    delta = min - rs;
            }

            RangeStart += delta;
            RangeEnd += delta;
            _absolutePosition = position;
            return;
        }

        var normalizedPosition = (position / size) * (Maximum - Minimum) + Minimum;

        if (_pointerManipulatingMin && normalizedPosition < RangeEnd)
        {
            var maxPos = Orientation == Orientation.Horizontal
                ? Canvas.GetLeft(_maxThumb)
                : DragHeight - Canvas.GetTop(_maxThumb);
            RangeStart = DragThumb(_minThumb, 0, maxPos, position, Orientation == Orientation.Vertical);
            UpdateToolTipText(RangeStart);
        }
        else if (_pointerManipulatingMax && normalizedPosition > RangeStart)
        {
            var minPos = Orientation == Orientation.Horizontal
                ? Canvas.GetLeft(_minThumb)
                : DragHeight - Canvas.GetTop(_minThumb);
            RangeEnd = DragThumb(_maxThumb, minPos, size, position, Orientation == Orientation.Vertical);
            UpdateToolTipText(RangeEnd);
        }
    }
    
    // Остальные поля и константы остаются без изменений
    private Rectangle _activeRectangle;
    private Thumb _minThumb;
    private Thumb _maxThumb;
    private Canvas _containerCanvas;
    private double _oldValue;
    private bool _valuesAssigned;
    private bool _minSet;
    private bool _maxSet;
    private bool _pointerManipulatingMin;
    private bool _pointerManipulatingMax;
    private bool _pointerManipulatingBoth;
    private double _absolutePosition;
    private Avalonia.Controls.Control _toolTip;
    private TextBlock _toolTipText;
    private const double Epsilon = 0.01;
    private bool _isDraggingStart;
    private bool _isDraggingEnd;
    private readonly DispatcherTimer _keyTimer = new DispatcherTimer();

    // private const string s_tpActiveRectangle = "ActiveRectangle";
    // private const string s_tpMinThumb = "MinThumb";
    // private const string s_tpMaxThumb = "MaxThumb";
    // private const string s_tpContainerCanvas = "ContainerCanvas";
    // private const string s_tpToolTipText = "ToolTipText";

    protected virtual void OnThumbDragStarted(VectorEventArgs e)
    {
        ThumbDragStarted?.Invoke(this, e);
    }

    protected virtual void OnThumbDragCompleted(VectorEventArgs e)
    {
        ThumbDragCompleted?.Invoke(this, e);
    }

    protected virtual void OnValueChanged(RangeChangedEventArgs e)
    {
        ValueChanged?.Invoke(this, e);
    }

    private void MinThumbDragStarted(object sender, VectorEventArgs e)
    {
        _isDraggingStart = true;
        OnThumbDragStarted(e);
        HandleThumbDragStarted(_minThumb);
    }

    private void MaxThumbDragStarted(object sender, VectorEventArgs e)
    {
        _isDraggingEnd = true;
        OnThumbDragStarted(e);
        HandleThumbDragStarted(_maxThumb);
    }

    private void HandleThumbDragCompleted(object sender, VectorEventArgs e)
    {
        _isDraggingStart = _isDraggingEnd = false;
        OnThumbDragCompleted(e);
        OnValueChanged(sender.Equals(_minThumb)
            ? new RangeChangedEventArgs(_oldValue, RangeStart, RangeSelectorProperty.RangeStartValue)
            : new RangeChangedEventArgs(_oldValue, RangeEnd, RangeSelectorProperty.RangeEndValue));
        SyncThumbs();

        if (_toolTip != null)
        {
            SetToolTipAt(sender as Thumb, false);
        }
    }

    private double DragThumb(Thumb thumb, double min, double max, double nextPos)
    {
        nextPos = Math.Max(min, nextPos);
        nextPos = Math.Min(max, nextPos);

        Canvas.SetLeft(thumb, nextPos);

        return Minimum + ((nextPos / DragWidth) * (Maximum - Minimum));
    }

    private void HandleThumbDragStarted(Thumb thumb)
    {
        var useMin = thumb == _minThumb;
        var otherThumb = useMin ? _maxThumb : _minThumb;

        _absolutePosition = Canvas.GetLeft(thumb);
        thumb.ZIndex = 10;
        otherThumb.ZIndex = 0;
        _oldValue = RangeStart;

        if (_toolTipText != null && _toolTip != null)
        {
            SetToolTipAt(thumb, true);

            UpdateToolTipText(useMin ? RangeStart : RangeEnd);
        }
    }

    private void MinThumbKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Left:
                RangeStart -= StepFrequency;
                SyncThumbs(fromMinKeyDown: true);

                SetToolTipAt(_minThumb, true);

                e.Handled = true;
                break;

            case Key.Right:
                RangeStart += StepFrequency;
                SyncThumbs(fromMinKeyDown: true);

                SetToolTipAt(_minThumb, true);

                e.Handled = true;
                break;
        }
    }

    private void MaxThumbKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Left:
                RangeEnd -= StepFrequency;
                SyncThumbs(fromMaxKeyDown: true);

                if (!ToolTip.GetIsOpen(_maxThumb))
                {
                    UnParentToolTip(_toolTip);
                    ToolTip.SetTip(_maxThumb, _toolTip);
                    ToolTip.SetIsOpen(_maxThumb, true);
                    ToolTip.SetPlacement(_maxThumb, PlacementMode.Top);
                    ToolTip.SetVerticalOffset(_maxThumb, -_containerCanvas.Bounds.Height);
                }

                e.Handled = true;
                break;
            case Key.Right:
                RangeEnd += StepFrequency;
                SyncThumbs(fromMaxKeyDown: true);

                if (!ToolTip.GetIsOpen(_maxThumb))
                {
                    UnParentToolTip(_toolTip);
                    ToolTip.SetTip(_maxThumb, _toolTip);
                    ToolTip.SetIsOpen(_maxThumb, true);
                    ToolTip.SetPlacement(_maxThumb, PlacementMode.Top);
                    ToolTip.SetVerticalOffset(_maxThumb, -_containerCanvas.Bounds.Height);
                }

                e.Handled = true;
                break;
        }
    }

    private void ThumbKeyUp(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Left:
            case Key.Right:
                if (_toolTip != null)
                {
                    _keyTimer.Debounce(() =>
                    {
                        SetToolTipAt(_minThumb, false);
                        SetToolTipAt(_maxThumb, false);
                    }, TimeSpan.FromSeconds(1));
                }

                e.Handled = true;
                break;
        }
    }

    private void ContainerCanvasPointerExited(object sender, PointerEventArgs e)
    {
        var position = e.GetCurrentPoint(_containerCanvas).Position.X;
        var normalizedPosition = ((position / DragWidth) * (Maximum - Minimum)) + Minimum;

        if (_pointerManipulatingMin)
        {
            _pointerManipulatingMin = false;
            _containerCanvas.IsHitTestVisible = true;
            OnValueChanged(new RangeChangedEventArgs(RangeStart, normalizedPosition,
                RangeSelectorProperty.RangeStartValue));
        }
        else if (_pointerManipulatingMax)
        {
            _pointerManipulatingMax = false;
            _containerCanvas.IsHitTestVisible = true;
            OnValueChanged(new RangeChangedEventArgs(RangeEnd, normalizedPosition,
                RangeSelectorProperty.RangeEndValue));
        }
    }

    private void ContainerCanvasPointerReleased(object sender, PointerReleasedEventArgs e)
    {
        _pointerManipulatingBoth = false;
        var position = e.GetCurrentPoint(_containerCanvas).Position.X;
        var normalizedPosition = ((position / DragWidth) * (Maximum - Minimum)) + Minimum;

        if (_toolTip != null)
        {
            var thumb = _pointerManipulatingMax ? _maxThumb : _pointerManipulatingMin ? _minThumb : null;
            if (thumb == null)
                return; // Should never happen, but just in case

            ToolTip.SetIsOpen(thumb, false);
            UnParentToolTip(_toolTip);
            ToolTip.SetTip(thumb, null);
        }

        if (_pointerManipulatingMin)
        {
            _pointerManipulatingMin = false;
            _containerCanvas.IsHitTestVisible = true;
            OnValueChanged(new RangeChangedEventArgs(RangeStart, normalizedPosition,
                RangeSelectorProperty.RangeStartValue));
        }
        else if (_pointerManipulatingMax)
        {
            _pointerManipulatingMax = false;
            _containerCanvas.IsHitTestVisible = true;
            OnValueChanged(new RangeChangedEventArgs(RangeEnd, normalizedPosition,
                RangeSelectorProperty.RangeEndValue));
        }

        SyncThumbs();
    }
    
    private void UpdateToolTipText(double newValue)
    {
        if (_toolTipText != null)
        {
            var format = ToolTipStringFormat ?? "0.##";
            _toolTipText.Text = newValue.ToString(format);
        }
    }

    private void VerifyValues()
    {
        if (Minimum > Maximum)
        {
            Minimum = Maximum;
            Maximum = Maximum;
        }

        if (Minimum == Maximum)
        {
            Maximum += Epsilon;
        }

        if (!_maxSet)
        {
            RangeEnd = Maximum;
        }

        if (!_minSet)
        {
            RangeStart = Minimum;
        }

        if (RangeStart < Minimum)
        {
            RangeStart = Minimum;
        }

        if (RangeEnd < Minimum)
        {
            RangeEnd = Minimum;
        }

        if (RangeStart > Maximum)
        {
            RangeStart = Maximum;
        }

        if (RangeEnd > Maximum)
        {
            RangeEnd = Maximum;
        }

        if (RangeEnd < RangeStart)
        {
            RangeStart = RangeEnd;
        }
    }

    private void RangeMinToStepFrequency()
    {
        RangeStart = MoveToStepFrequency(RangeStart);
    }

    private void RangeMaxToStepFrequency()
    {
        RangeEnd = MoveToStepFrequency(RangeEnd);
    }

    private double MoveToStepFrequency(double rangeValue)
    {
        double newValue = Minimum + (((int)Math.Round((rangeValue - Minimum) / StepFrequency)) * StepFrequency);

        if (newValue < Minimum)
        {
            return Minimum;
        }
        else if (newValue > Maximum || Maximum - newValue < StepFrequency)
        {
            return Maximum;
        }
        else
        {
            return newValue;
        }
    }

    private void ContainerCanvasSizeChanged(object sender, SizeChangedEventArgs e)
    {
        SyncThumbs();
    }

    private void SetToolTipAt(Thumb thumb, bool open)
    {
        if (!ShowValueToolTip)
            return;

        if (open && !ToolTip.GetIsOpen(thumb))
        {
            UnParentToolTip(_toolTip);
            ToolTip.SetTip(thumb, _toolTip);
            ToolTip.SetIsOpen(thumb, true);
            ToolTip.SetPlacement(thumb, PlacementMode.Top);
            ToolTip.SetVerticalOffset(thumb, -_containerCanvas.Bounds.Height);
        }
        else if (!open)
        {
            ToolTip.SetIsOpen(thumb, false);
            UnParentToolTip(_toolTip);
            ToolTip.SetTip(thumb, null);
        }
    }

    private static void UnParentToolTip(Avalonia.Controls.Control c)
    {
        if (c.Parent is Panel p)
        {
            p.Children.Remove(c);
        }
        else if (c.Parent is ContentControl cc)
        {
            cc.Content = null;
        }
        else if (c.Parent is Decorator d)
        {
            d.Child = null;
        }
    }
}

internal static class DispatcherTimerExtensions
{
    public static void Debounce(this DispatcherTimer timer, Action action, TimeSpan interval, bool immediate = false)
    {
        // Check and stop any existing timer
        var timeout = timer.IsEnabled;
        if (timeout)
        {
            timer.Stop();
        }

        // Reset timer parameters
        timer.Tick -= TimerTick;
        timer.Interval = interval;

        if (immediate)
        {
            // If we're in immediate mode then we only execute if the timer wasn't running beforehand
            if (!timeout)
            {
                action.Invoke();
            }
        }
        else
        {
            // If we're not in immediate mode, then we'll execute when the current timer expires.
            timer.Tick += TimerTick;

            // Store/Update function
            _debounceInstances.AddOrUpdate(timer, action, (k, v) => action);
        }

        // Start the timer to keep track of the last call here.
        timer.Start();
    }

    private static void TimerTick(object sender, object e)
    {
        // This event is only registered/run if we weren't in immediate mode above
        if (sender is DispatcherTimer timer)
        {
            timer.Tick -= TimerTick;
            timer.Stop();

            if (_debounceInstances.TryRemove(timer, out Action action))
            {
                action?.Invoke();
            }
        }
    }

    private static ConcurrentDictionary<DispatcherTimer, Action> _debounceInstances =
        new ConcurrentDictionary<DispatcherTimer, Action>();
}