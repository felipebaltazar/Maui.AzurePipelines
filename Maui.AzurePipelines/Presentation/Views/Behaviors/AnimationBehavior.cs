namespace PipelineApproval.Presentation.Views.Behaviors;

public class AnimationBehavior : Behavior<Label>
{
    private bool animating;
    private bool isActive;

    public bool IsActive
    {
        get => isActive;
        set
        {
            isActive = value;
            OnActiveChanged();
        }
    }

    public Label Label { get; private set; }

    private void OnActiveChanged()
    {
        if (IsActive)
        {
            animating = true;

            async Task animateLabel()
            {
                while (animating)
                {
                    if (Label.Text != Icons.Running)
                    {
                        animating = false;
                        break;
                    }

                    await Label.RotateTo(180, 500);
                    await Label.RotateTo(360, 500);
                    await Label.RotateTo(0, 10);
                }
            }

            _ = Task.Run(animateLabel);
        }
    }

    protected override void OnAttachedTo(Label label)
    {
        base.OnAttachedTo(label);
        Label = label;
    }

    protected override void OnDetachingFrom(BindableObject bindable)
    {
        base.OnDetachingFrom(bindable);
        animating = false;
    }
}
