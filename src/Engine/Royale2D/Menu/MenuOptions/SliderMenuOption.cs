namespace Royale2D
{
    // For things like volume control
    public class SliderMenuOption : MenuOption
    {
        int sliderValue;
        int minSliderValue;
        int maxSliderValue;
        Action<int>? setSliderValue;
        public SliderMenuOption(string text, int sliderValue, int minSliderValue, int maxSliderValue, Action<int>? setSliderValue = null, Action? selectAction = null) : base(text)
        {
            this.sliderValue = sliderValue;
            this.minSliderValue = minSliderValue;
            this.maxSliderValue = maxSliderValue;
            this.setSliderValue = setSliderValue;
            this.selectAction = selectAction ?? this.selectAction;
        }

        public override void Update()
        {
            base.Update();

            if (Game.input.IsHeld(Control.MenuLeft) && sliderValue > minSliderValue)
            {
                sliderValue--;
                setSliderValue?.Invoke(sliderValue);
            }
            else if (Game.input.IsHeld(Control.MenuRight) && sliderValue < maxSliderValue)
            {
                sliderValue++;
                setSliderValue?.Invoke(sliderValue);
            }
            else if (Game.input.IsPressed(Control.MenuSelectPrimary))
            {
                selectAction.Invoke();
            }
        }

        public override void Render(Drawer drawer, int x, int y)
        {
            drawer.DrawText(string.Format(text, sliderValue.ToString()), x, y);
        }
    }
}
