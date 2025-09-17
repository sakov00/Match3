using _Project.Scripts.UI.DraggableObjects.Draggable;

namespace _Project.Scripts.UI.DraggableObjects.PlayableBlock
{
    public class PlayableBlockView : DraggableView
    {
        public override DraggablePresenter BasePresenter { get; protected set; }

        public override void Initialize(DraggablePresenter presenter)
        {
            BasePresenter = presenter;
        }
    }
}