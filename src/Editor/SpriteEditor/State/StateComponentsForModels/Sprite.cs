using Editor;
using Shared;

namespace SpriteEditor;

public class Sprite : StateComponentWithModel<SpriteModel>
{
    #region model
    public string name { get => TrGetD(model.name); set => TrSet(value); }
    public TrackableList<Frame> frames { get => TrListGet(model.frames.Select(f => new Frame(context, f))); init => TrListSet(value); }
    public TrackableList<Hitbox> hitboxes { get => TrListGet(model.hitboxes.Select(h => new Hitbox(context, h))); init => TrListSet(value); }
    public int loopStartFrame { get => TrGetD(model.loopStartFrame); set => TrSet(value); }
    public string alignment { get => TrGetD(model.alignment); set => TrSetC(value, OnAlignmentChange); }

    // When alignment changes, adjust child box and POI positions as well to match
    public void OnAlignmentChange(string oldAlignment, string newAlignment)
    {
        bool isFirstFrame = true;
        foreach (Frame frame in frames)
        {
            (int oldCx, int oldCy) = Alignment.GetAlignmentOrigin(oldAlignment, frame.rect.w, frame.rect.h);
            (int newCx, int newCy) = Alignment.GetAlignmentOrigin(newAlignment, frame.rect.w, frame.rect.h);
            int deltaX = oldCx - newCx;
            int deltaY = oldCy - newCy;

            // For the global hitboxes, use the first frame for the alignment rect w/h
            if (isFirstFrame)
            {
                foreach (Hitbox hitbox in hitboxes)
                {
                    hitbox.Move(deltaX, deltaY);
                }
            }

            foreach (Hitbox hitbox in frame.hitboxes)
            {
                hitbox.Move(deltaX, deltaY);
            }

            foreach (Drawbox drawbox in frame.drawboxes)
            {
                drawbox.Move(deltaX, deltaY);
            }

            foreach (POI poi in frame.POIs)
            {
                poi.Move(deltaX, deltaY);
            }

            isFirstFrame = false;
        }
    }
    public string wrapMode { get => TrGetD(model.wrapMode); set => TrSet(value); }
    public string spritesheetName { get => TrGetD(model.spritesheetName); set => TrSet(value); }
    
    public SpriteModel ToModel()
    {
        return new SpriteModel(
            name,
            spritesheetName,
            frames.SelectList(f => f.ToModel()),
            hitboxes.SelectList(h => h.ToModel()),
            loopStartFrame,
            alignment,
            wrapMode
        );
    }
    #endregion

    public bool isDirty { get => TrGet<bool>(); set => TrSet(value, [nameof(displayName)]); }
    public Spritesheet GetSpritesheet(State state) => state.spritesheets.First(s => s.name == spritesheetName);
    public string displayName => name + (isDirty ? "*" : "");
    public override string ToString() => name;

    public Frame? selectedFrame { get => TrGet<Frame?>(); set => TrSetAll(value, editorEvent: EditorEvent.SESelectedFrameChange, onChangeCallback: OnSelectedFrameChange); }
    public void OnSelectedFrameChange(Frame? oldFrame, Frame? newFrame)
    {
        if (oldFrame != null) oldFrame.selected = false;
        if (newFrame != null) newFrame.selected = true;
    }

    public Sprite(EditorContext context, SpriteModel model) : base(context, model)
    {
        selectedFrame = frames.SafeGet(0);
    }

    public Sprite(EditorContext context, string name, string spritesheetName) : 
        this(context, SpriteModel.New(name, spritesheetName))
    {

    }

    public void Draw(State state, Drawer drawer, int frameIndex, float x, float y, float alpha = 1)
    {
        Frame frame = this.frames[frameIndex];

        MyRect rect = frame.rect;
        MyPoint offset = this.GetAlignOffset(frameIndex);

        List<Tuple<Action, float>> wrappers = new List<Tuple<Action, float>>();
        wrappers.Add(new Tuple<Action, float>(
            () =>
            {
                drawer.DrawImage( 
                    GetSpritesheet(state).drawer,
                    x + offset.x + frame.offset.x, // dx
                    y + offset.y + frame.offset.y, // dy
                    rect.x1,                       // sx
                    rect.y1,                       // sy
                    rect.w,                        // sw
                    rect.h,                        // sh
                    alpha);
            },
            0
        ));

        wrappers.Sort((a, b) =>
        {
            return Math.Sign(a.Item2 - b.Item2);
        });

        foreach (var wrapper in wrappers) 
        {
            wrapper.Item1.Invoke();
        }
    }

    //Returns actual width and heights, not 0-1 number
    public MyPoint GetAlignOffset(int frameIndex)
    {
        Frame frame = frames[frameIndex];
        MyRect rect = frame.rect;

        int w = rect.w;
        int h = rect.h;

        int halfW = w / 2;
        int halfH = h / 2;

        halfW = MyMath.Floor(halfW);
        halfH = MyMath.Floor(halfH);

        int x;
        int y;

        if (alignment == Alignment.TopLeft) { x = 0; y = 0; }
        else if (alignment == Alignment.TopMid) { x = -halfW; y = 0; }
        else if (alignment == Alignment.TopRight) { x = -w; y = 0; }
        else if (alignment == Alignment.MidLeft) { x = 0; y = -halfH; }
        else if (alignment == Alignment.Center) { x = -halfW; y = -halfH; }
        else if (alignment == Alignment.MidRight) { x = -w; y = -halfH; }
        else if (alignment == Alignment.BotLeft) { x = 0; y = -h; }
        else if (alignment == Alignment.BotMid) { x = -halfW; y = -h; }
        else if (alignment == Alignment.BotRight) { x = -w; y = -h; }
        else { throw new Exception("No alignment provided"); }

        return new MyPoint(x, y);
    }
}
